using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NGeoHash;
using QianMo.Core.Cluster;

namespace QianMo.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataReader = new DataFileReader();
            var pltFiles = dataReader.GetDataFiles("../../data/001/", "*.plt");
            var tracks = dataReader.GetTrajectories(pltFiles);

            Parallel.ForEach(tracks, t =>
            {
//                t.GeoCodes = GeoHelper.GetGeoCodes(t.GeoPoints, 3).ToList();
                t.GeoCodes = t.GeoPoints.Select(tt => GeoHash.Encode(tt.Latitude, tt.Longitude, 7))
                    .Distinct()
                    .ToList();
            });


            Console.WriteLine(tracks.Count());

            var cluster = new CommonSubsequenceCluster();
            var sh = new Stopwatch();
            sh.Start();
            var tree = cluster.BuildClusterTree(tracks.ToArray(), 0.7f, 0.4f);
            Console.WriteLine($"BuildClusterTree, count:{tracks.Count()}, time:{sh.Elapsed}");

            var drawer = new TrajectoryDrawer(128, 128);

            Directory.GetFiles("img", "*.png")
                .ToList()
                .ForEach(File.Delete);

            Directory.CreateDirectory("img");

            cluster.ForeachTree(tree, node =>
            {
                var bitmap = drawer.CreatBitmap(node);
                bitmap.Save($"img/{node.LevelTag}.png");
                bitmap.Dispose();
//                Console.WriteLine(node.LevelTag);
                
                foreach (var sib in node.Siblings)
                {
                    var sibBit = drawer.CreatBitmap(sib);
                    sibBit.Save($"img/{sib.LevelTag}.png");
                    sibBit.Dispose();
//                    Console.WriteLine(sib.LevelTag);
                }
            });
        }
    }
}