using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QianMo.Core.Cluster;
using QianMo.Core.Infrastructure;

namespace QianMo.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataReader = new DataFileReader();
            var pltFiles = dataReader.GetDataFiles("../../data/001/", "*.plt");
            var tracks = dataReader.GetTrajectories(pltFiles);
            tracks.ToList().ForEach(t => t.GeoCodes = GeoHelper.GetGeoCodes(t.GeoPoints, 3).ToList());
            Console.WriteLine(tracks.Count());

            var cluster = new CommonSubsequenceCluster();
            var tree = cluster.BuildClusterTree(tracks.ToArray(), 0.8f, 0.4f);

            var drawer = new TrajectoryDrawer(128, 128);
            Directory.CreateDirectory("img");

            cluster.ForeachTree(tree, node =>
            {
                var bitmap = drawer.CreatBitmap(node);
                bitmap.Save($"img/{node.LevelTag}.png");
                bitmap.Dispose();

                foreach (var sib in node.Siblings)
                {
                    var sibBit = drawer.CreatBitmap(sib);
                    sibBit.Save($"img/{sib.LevelTag}.png");
                    sibBit.Dispose();
                }
            });
        }
    }
}