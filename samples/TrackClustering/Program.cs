using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;
using NGeoHash;
using QianMo.Core.Clustering;
using QianMo.Core.Infrastructure.Data;
using QianMo.Core.Infrastructure.Drawing;
using QianMo.Core.Models;
using QianMo.Core.Utilities;

namespace TrackClustering
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataReader = new DataFileReader<GeoInfoModel>();
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "../../../../../data/001/");
            var imgDir = Path.Combine(Directory.GetCurrentDirectory(), "img");
            var pltFiles = FileTool.GetAllFile(dir, "*.plt");
            var tracks = dataReader.GetTrajectories(pltFiles);

            Parallel.ForEach(tracks, t =>
            {
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

            if (Directory.Exists(imgDir))
                Directory.GetFiles(imgDir, "*.png")
                    .ToList()
                    .ForEach(File.Delete);
            else
                Directory.CreateDirectory(imgDir);

            cluster.ForeachTree(tree, node =>
            {
                var draw = new DrawBase(256, 256)
                    .OpenAutoFit(node.MinLon, node.MinLat, node.MaxLon, node.MaxLat)
                    .Draw(node, Color.Red, 3);
                draw.Image.Save($"{imgDir}/{node.LevelTag}.png");
                draw.Image.Dispose();
                //                Console.WriteLine(node.LevelTag);

                foreach (var sib in node.Siblings)
                {
                    var drawSib = new DrawBase(256, 256)
                        .OpenAutoFit(sib.MinLon, sib.MinLat, sib.MaxLon, sib.MaxLat)
                        .Draw(sib, Color.Red, 3);
                    drawSib.Image.Save($"{imgDir}/{sib.LevelTag}.png");
                    drawSib.Image.Dispose();
                    //                    Console.WriteLine(sib.LevelTag);
                }
            });
        }
    }
}