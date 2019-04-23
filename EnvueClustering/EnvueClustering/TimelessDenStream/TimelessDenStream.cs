using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvueClustering.ClusteringBase;
using EnvueClustering.Exceptions;

namespace EnvueClustering.TimelessDenStream
{
    public class TimelessDenStream<T> where T : ITransformable<T>, IIdentifiable
    {
        // DenStream parameters
        private const float MAX_RADIUS = 15;
        
        // DBSCAN parameters
        private const float EPS = 250, MIN_POINTS = 2;
        
        // Data stream and micro cluster map
        private readonly List<UntimedMicroCluster<T>> _microClusters;
        private ConcurrentQueue<T> _dataStream;

        // DBSCAN instance
        private IClusterable<UntimedMicroCluster<T>> _dbscan;
        
        // Similarity functions
        private Func<T, T, float> _pointSimilarityFunction;
        private Func<UntimedMicroCluster<T>, UntimedMicroCluster<T>, float> _microClusterSimilarityFunction;
        
        // Local control variables
        private bool _userTerminated, _clusteringInProgress;

        public TimelessDenStream(
            Func<T, T, float> pointSimilarityFunction,
            Func<UntimedMicroCluster<T>, UntimedMicroCluster<T>, float> microClusterSimilarityFunction)
        {
            _pointSimilarityFunction = pointSimilarityFunction;
            _microClusterSimilarityFunction = microClusterSimilarityFunction;

            _microClusters = new List<UntimedMicroCluster<T>>();
            _dataStream = new ConcurrentQueue<T>();
        }

        public void Add(T dataPoint)
        {
            _dataStream.Enqueue(dataPoint);
        }

        public void Add(IEnumerable<T> dataPoints)
        {
            foreach (var dataPoint in dataPoints)
                Add(dataPoint);
        }

        public void Remove(T dataPoint)
        {
            foreach (var microCluster in _microClusters)
            {
                microCluster.Points.RemoveAll(p => p.Id == dataPoint.Id);
            }
        }

        /// <summary>
        /// Launches the MaintainClusterMap algorithm in a background thread.
        /// </summary>
        /// <returns>An action that, when called, terminates the thread.</returns>
        /// <exception cref="DenStreamUninitializedDataStreamException">If the data stream has not been initialized with
        /// SetDataStream() before calling, this exception is thrown.</exception>
        public Action MaintainClusterMap()
        {
            if (_dataStream == null)
            {
                throw new DenStreamUninitializedDataStreamException(
                    $"The shared data stream resource has not been initialized.\n " +
                    $"Use the SetDataStream() method to initialize the data stream before calling.\n ");
            }
            
            var maintainClusterMapThread = Task.Run(() => MaintainClusterMapAsync());  // Run in background thread
            return () =>
            {
                _userTerminated = true;
                _dataStream = null;
                maintainClusterMapThread.Wait();
            };  // Return an action to force the thread to terminate
        }

        private void MaintainClusterMapAsync()
        {
            while (_dataStream != null)
            {
                if (_userTerminated)
                    return;
                
                // Get the next data point in the data stream
                var successfulDequeue = _dataStream.TryDequeue(out var p);
                if (!successfulDequeue || _clusteringInProgress)
                    continue;
                
                // Merge the dataPoint into the micro cluster map
                Merge(p);
            }
        }

        private void Merge(T p)
        {
            // Sort micro clusters by distance to p
            _microClusters.Sort((u, v) =>
                _pointSimilarityFunction(u.Center, p)
                    .CompareTo(_pointSimilarityFunction(v.Center, p)));

            var closestMicroCluster = _microClusters.First();
            
            // Try to insert the point into this cluster
            var successfulInsert = TryInsert(p, closestMicroCluster, 
                (mc) => mc.Radius <= MAX_RADIUS);
            
            if (!successfulInsert)
            {
                // Create a new micro cluster, add to the cluster map
                _microClusters.Add(new UntimedMicroCluster<T>(
                    new [] {p}, _pointSimilarityFunction));
            }
        }
        
        /// <summary>
        /// Attempts to insert a point p into a cluster.
        /// The success of the insertion is based on a predicate on the cluster.
        /// If the predicate succeeds after inserting p into the cluster, the method
        /// returns true. If the predicate fails, p is removed from the cluster and
        /// the method returns false.
        /// </summary>
        /// <param name="p">The point to insert in the cluster.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="predicate">The predicate that must pass before insertion is legal.</param>
        /// <returns></returns>
        private bool TryInsert(T p, UntimedMicroCluster<T> cluster, Predicate<UntimedMicroCluster<T>> predicate)
        {
            cluster.Points.Add(p);
            if (predicate(cluster))
            {
                return true;
            }

            cluster.Points.Remove(p);
            return false;
        }

    }
}