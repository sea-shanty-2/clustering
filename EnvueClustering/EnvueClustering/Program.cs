﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using EnvueClustering.ClusteringBase;
using EnvueClustering.Data;
using EnvueClustering.Exceptions;
using Newtonsoft.Json;

namespace EnvueClustering
{
    class Program
    {
        static void Main(string[] args)
        {
            ShrinkageClusteringTest();
            
        }

        private static void ShrinkageClusteringTest()
        {
            var filePath = $"{Environment.CurrentDirectory}/Data/Synthesis/DataSteamGenerator/data.synthetic.json";
            var dataStream = ContinuousDataReader.ReadSyntheticEuclidean(filePath);

            Func<EuclideanPoint, EuclideanPoint, float> simFunc = (x, y) => 
                (float)Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));


            Func<CoreMicroCluster<EuclideanPoint>, CoreMicroCluster<EuclideanPoint>, int, float> cmcSimFunc = (u, v, t) =>
                (float) Math.Sqrt(Math.Pow(u.Center(t).X - v.Center(t).X, 2) +
                                  Math.Pow(u.Center(t).Y - v.Center(t).Y, 2));
            
            var denStream = new DenStream<EuclideanPoint>(simFunc, cmcSimFunc);
            denStream.AddToDataStream(dataStream);
            var terminate = denStream.MaintainClusterMap();
            Thread.Sleep(2000);
            var clusters = denStream.Cluster();
            terminate();
            
            var sc = new ShrinkageClustering<EuclideanPoint>(100, 1000, simFunc);

            var scClustersAll = new List<dynamic>();

            var numClusters = 0;
            foreach (var cluster in clusters)
            {
                var scClusters = sc.Cluster(cluster);
                foreach (var scCluster in scClusters)
                {
                    var k = numClusters;
                    scClustersAll.AddRange(scCluster.Select(p => new { x = p.X, y = p.Y, c = k}));
                }
                numClusters++;
            }
            
            File.WriteAllText(
                $"{Environment.CurrentDirectory}/Data/Synthesis/ClusterVisualization/dbscanVisu/sc.json", 
                JsonConvert.SerializeObject(scClustersAll));
        }

        private static void DenStreamAsyncTest()
        {
            var filePath = $"{Environment.CurrentDirectory}/Data/Synthesis/DataSteamGenerator/data.synthetic.json";
            var dataStream = ContinuousDataReader.ReadSyntheticEuclidean(filePath);

            Func<EuclideanPoint, EuclideanPoint, float> simFunc = (x, y) => 
                (float)Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));


            Func<CoreMicroCluster<EuclideanPoint>, CoreMicroCluster<EuclideanPoint>, int, float> cmcSimFunc = (u, v, t) =>
                (float) Math.Sqrt(Math.Pow(u.Center(t).X - v.Center(t).X, 2) +
                                  Math.Pow(u.Center(t).Y - v.Center(t).Y, 2));
            
            var denStream = new DenStream<EuclideanPoint>(simFunc, cmcSimFunc);
            denStream.AddToDataStream(dataStream);
            var terminate = denStream.MaintainClusterMap();
            foreach (var i in 1000.Range())
            {
                try
                {
                    Thread.Sleep(5);
                    var clusters = denStream.Cluster();
                    Console.WriteLine($"{clusters.Length} clusters.");
                }
                catch (EnvueArgumentException e)
                {
                    Console.WriteLine($"{i} threw exception.");
                }
                
            }
            
            Console.WriteLine($"Terminating MaintainClusterMap");
            terminate();
        }

        private static void DbScanSyntheticTest()
        {
            var filePath = $"{Environment.CurrentDirectory}/Data/Synthesis/DataSteamGenerator/data.synthetic.json";
            var dataStream = ContinuousDataReader.ReadSyntheticEuclidean(filePath);

            Func<EuclideanPoint, EuclideanPoint, float> simFunc = (x, y) => 
                (float)Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));


            Func<CoreMicroCluster<EuclideanPoint>, CoreMicroCluster<EuclideanPoint>, int, float> cmcSimFunc = (u, v, t) =>
                (float) Math.Sqrt(Math.Pow(u.Center(t).X - v.Center(t).X, 2) +
                                  Math.Pow(u.Center(t).Y - v.Center(t).Y, 2));
            
            var denStream = new DenStream<EuclideanPoint>(simFunc, cmcSimFunc);
            //denStream.SetDataStream(dataStream);
            var terminate = denStream.MaintainClusterMap();
            Thread.Sleep(2000);
            var clusters = denStream.Cluster();

            var clusterPoints = new List<dynamic>();
            foreach (var (i, cluster) in clusters.Enumerate())
            {
                foreach (var point in cluster)
                {
                    clusterPoints.Add(new {x = point.X, y = point.Y, c = i});
                }
            }

            Console.WriteLine($"Waiting...");
            Thread.Sleep(2000);
            Console.WriteLine($"Terminating MaintainClusterMap");
            terminate();
        }

        private static void DenStreamSyntheticTest()
        {
            var filePath = $"{Environment.CurrentDirectory}/Data/Synthesis/DataSteamGenerator/data.synthetic.json";
            var dataStream = ContinuousDataReader.ReadSyntheticEuclidean(filePath);

            Func<EuclideanPoint, EuclideanPoint, float> simFunc = (x, y) => 
                (float)Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));
            
            Func<CoreMicroCluster<EuclideanPoint>, CoreMicroCluster<EuclideanPoint>, int, float> cmcSimFunc = (u, v, t) =>
                (float) Math.Sqrt(Math.Pow(u.Center(t).X - v.Center(t).X, 2) +
                                  Math.Pow(u.Center(t).Y - v.Center(t).Y, 2));
            
            var denStream = new DenStream<EuclideanPoint>(simFunc, cmcSimFunc);
            denStream.AddToDataStream(dataStream);
            denStream.MaintainClusterMap();
            
            var pcmcs = new List<EuclideanPoint>();
            var ocmcs = new List<EuclideanPoint>();
            var pcmcPoints = new List<EuclideanPoint>();
            var ocmcPoints = new List<EuclideanPoint>();

            foreach (var pcmc in denStream.PotentialCoreMicroClusters)
            {
                var _pcmc = pcmc as CoreMicroCluster<EuclideanPoint>;
                var p = new EuclideanPoint(_pcmc.Center(denStream.CurrentTime).X, _pcmc.Center(denStream.CurrentTime).Y,
                    (int) _pcmc.Radius(denStream.CurrentTime));
                p.SetRadius(_pcmc.Radius(denStream.CurrentTime));
                pcmcs.Add(p);
            }
            
            foreach (var ocmc in denStream.OutlierCoreMicroClusters)
            {                
                var _ocmc = ocmc as CoreMicroCluster<EuclideanPoint>;
                var p = new EuclideanPoint(_ocmc.Center(denStream.CurrentTime).X, _ocmc.Center(denStream.CurrentTime).Y,
                    (int) _ocmc.Radius(denStream.CurrentTime));
                p.SetRadius(_ocmc.Radius(denStream.CurrentTime));
                ocmcs.Add(p);
            }

            foreach (var pcmc in denStream.PotentialCoreMicroClusters)
            {
                pcmc.Points.ForEach(p =>
                {
                    var ep = new EuclideanPoint(p.X, p.Y, 2);
                    ep.SetRadius(2);
                    pcmcPoints.Add(ep);
                });
            }
            
            foreach (var ocmc in denStream.OutlierCoreMicroClusters)
            {
                ocmc.Points.ForEach(p =>
                {
                    var ep = new EuclideanPoint(p.X, p.Y, 2);
                    ep.SetRadius(2);
                    ocmcPoints.Add(ep);
                });
            }

            File.WriteAllText($"{Environment.CurrentDirectory}/Data/Synthesis/ClusterVisualization/pcmcs.json", JsonConvert.SerializeObject(pcmcs));
            File.WriteAllText($"{Environment.CurrentDirectory}/Data/Synthesis/ClusterVisualization/ocmcs.json", JsonConvert.SerializeObject(ocmcs));
            File.WriteAllText($"{Environment.CurrentDirectory}/Data/Synthesis/ClusterVisualization/pcmcPoints.json", JsonConvert.SerializeObject(pcmcPoints));
            File.WriteAllText($"{Environment.CurrentDirectory}/Data/Synthesis/ClusterVisualization/ocmcPoints.json", JsonConvert.SerializeObject(ocmcPoints));

            Console.WriteLine($"Wrote {pcmcs.Count} PCMCs and {ocmcs.Count} OCMCs to disk.");
        }
    }
}