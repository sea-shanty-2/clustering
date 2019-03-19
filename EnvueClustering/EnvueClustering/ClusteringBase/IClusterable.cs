using System;
using System.Collections.Generic;

namespace EnvueClustering.ClusteringBase
{
    public interface IClusterable<T>
    {
        /// <summary>
        /// Clusters a stream of data points. 
        /// </summary>
        /// <param name="dataStream">A stream of data points paired with a timestamp.</param>
        /// <param name="similarityFunction"></param>
        /// <returns></returns>
        T[][] Cluster(IEnumerable<(T, float)> dataStream, Func<T, T, float> similarityFunction);
    }
}