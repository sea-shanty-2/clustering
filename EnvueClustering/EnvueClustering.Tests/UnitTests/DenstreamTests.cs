using System.Collections;
using System.Collections.Generic;
using EnvueClustering.ClusteringBase;
using EnvueClustering.TimelessDenStream;
using EnvueClusteringAPI.Models;
using NUnit.Framework;

namespace EnvueClustering.Tests
{
    [TestFixture]
    public class DenstreamTests
    {
        private TimelessDenStream<Streamer> _denStream;

        [SetUp]
        public void SetupDenstream()
        {
            _denStream = new TimelessDenStream<Streamer>(
                Similarity.Haversine, 
                Similarity.Haversine);
        }

        [Test]
        public void AddEnumerable_AddOnePoint_OneCluster()
        {
            List<Streamer> streamers = new List<Streamer>();
            streamers.Add(new Streamer(10, 20, new float[]{1, 0, 1}, 0, "Test"));
            _denStream.Add(streamers);

            _denStream.Cluster();

            Assert.AreEqual(1, _denStream.MicroClusters.Count);
        }

        [Test]
        public void AddEnumerable_AddTwoPoints_OneCluster()
        {
            List<Streamer> streamers = new List<Streamer>();
            streamers.Add(new Streamer(10, 20, new float[]{1, 0, 1}, 0, "Test"));
            streamers.Add(new Streamer(10, 22, new float[]{1, 0, 1}, 1, "Test2"));
            
            _denStream.Add(streamers);
            _denStream.Cluster();
            
            Assert.AreEqual(1, _denStream.MicroClusters.Count);
        }
        
        [Test]
        public void AddEnumerable_AddTwoPoints_TwoClusters()
        {
            List<Streamer> streamers = new List<Streamer>();
            streamers.Add(new Streamer(10, 20, new float[]{1, 0, 1}, 0, "Test"));
            streamers.Add(new Streamer(0, 100, new float[]{0, 1, 1}, 1, "Test2"));
            
            _denStream.Add(streamers);
            _denStream.Cluster();
            
            Assert.AreEqual(2, _denStream.MicroClusters.Count);
        }

        [Test]
        public void AddSingle_AddOnePoint_NoClusters()
        {
            _denStream.Add(new Streamer(10, 20, new float[]{1, 0, 1}, 0, "Test"));
            _denStream.Cluster();
            
            Assert.AreEqual(1, _denStream.MicroClusters.Count);
        }
    }
}