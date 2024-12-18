using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NGeoHash;
using QianMo.Core.Algorithms;
using QianMo.Core.Clustering;
using QianMo.Core.Infrastructure.Data;
using QianMo.Core.Models;
using QianMo.Core.Utilities;
using QianMo.Tool.Drawing;
using SixLabors.ImageSharp;

namespace TrackClustering
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataReader = new DataFileReader<GeoInfoModel>();
            var dir = "../../../../../data";
            var pltFiles = FileTool.GetAllFile(dir, "*.plt");
            var tracks = dataReader.GetTrajectories(pltFiles)
            .DistinctBy(t => t.Name)
            .ToList();
            using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            // TrackClusteringTest(tracks);
            SpatioTemporalCoexistenceTest(tracks, factory);
        }
        static void SpatioTemporalCoexistenceTest(IList<Trajectory> tracks, ILoggerFactory factory)
        {
            var imgDir = Path.Combine(Directory.GetCurrentDirectory(), "img-stc");
            if (Directory.Exists(imgDir))
                Directory.Delete(imgDir, true);
            Directory.CreateDirectory(imgDir);
            var logger = factory.CreateLogger<SpatioTemporalCoexistence>();
            var stc = new SpatioTemporalCoexistence(logger);
            TimeSpan timeThreshold = TimeSpan.FromSeconds(3);
            ConcurrentBag<TwoTrajectoryGroup> groups = [];
            Parallel.For(0, tracks.Count, i =>
            // for (int i = 0; i < tracks.Count; i++)
            {
                var tracki = tracks[i];
                for (int j = i + 1; j < tracks.Count; j++)
                {
                    var trackj = tracks[j];
                    (List<IGeoInfoModel>, List<IGeoInfoModel>)? v = stc.CheckForBoxIntersection(tracki, trackj, timeThreshold);
                    if (v == null) continue;
                    var now = DateTime.Now;
                    int step = Math.Max(1, (int)(Math.Min(tracki.GeoPoints.Count, trackj.GeoPoints.Count) * 0.007));
                    var timess = stc.FindCompanionSegments(v.Value.Item1, v.Value.Item2, timeThreshold, 0.001, step);
                    logger.LogDebug("find_companion_segments:{},{},{}", i, j, DateTime.Now - now);
                    if (timess == null) continue;
                    logger.LogInformation("coexistence:{},{},{},{}", i, j, tracki.Name, trackj.Name);
                    var mergedTimes = stc.MergeTimes(timess, 60 * 3);
                    var filteredList = mergedTimes.Where(tup => (tup.Item2 - tup.Item1).TotalMinutes > 3).ToList();
                    if (filteredList.Count == 0) continue;
                    //     var draw = new DrawBase(256, 256)
                    //   .OpenAutoFit(
                    // Math.Min(tracki.MinLon, trackj.MinLon),
                    //     Math.Min(tracki.MinLat, trackj.MinLat),
                    //     Math.Max(tracki.MaxLon, trackj.MaxLon),
                    //    Math.Max(tracki.MaxLat, trackj.MaxLat))
                    //   .Draw(tracki, Color.Red, 3)
                    //   .Draw(trackj, Color.Green, 3);
                    //     draw.Image.Save($"{imgDir}/{tracki.Name}-{trackj.Name}.png");
                    var draw = new DrawBase(512, 512);
                    draw.OpenAutoFit(
                        Math.Min(tracki.MinLon, trackj.MinLon),
                        Math.Min(tracki.MinLat, trackj.MinLat),
                        Math.Max(tracki.MaxLon, trackj.MaxLon),
                        Math.Max(tracki.MaxLat, trackj.MaxLat)
                        );
                    var c1 = Color.Red;
                    var c2 = Color.Green;
                    c1 = c1.WithAlpha(0.5f);
                    c2 = c2.WithAlpha(0.5f);
                    var countHasPoints = 0;
                    for (int k = 0; k < filteredList.Count; k++)
                    {
                        var (st, et) = filteredList[k];
                        var tracki_1 = new Trajectory();
                        tracki_1.Init(tracki.GeoPoints.Where(t => t.Time >= st && t.Time <= et).ToList());
                        var trackj_1 = new Trajectory();
                        trackj_1.Init(trackj.GeoPoints.Where(t => t.Time >= st && t.Time <= et).ToList());
                        if (tracki_1.GeoPoints.Count > 0) countHasPoints++;
                        if (trackj_1.GeoPoints.Count > 0) countHasPoints++;
                        draw.Draw(tracki_1, c1, 3)
                       .Draw(trackj_1, c2, 3);

                    }
                
                    if (countHasPoints < 2) continue;
                    groups.Add(new TwoTrajectoryGroup
                    {
                        Name1 = tracki.Name,
                        Name2 = trackj.Name,
                        Start = filteredList.Max(t => t.Item1),
                        End = filteredList.Min(t => t.Item2)
                    });

                    draw.Image.Save($"{imgDir}/{tracki.Name}-{trackj.Name}_0.png");
                }
            });
            stc.GenerateGroupName([.. groups]);
            var dict = groups.GroupBy(t => t.Group).ToList();
            foreach (var items in dict)
            {
                var names = new HashSet<string>();
                foreach (var item in items)
                {
                    names.Add(item.Name1);
                    names.Add(item.Name2);
                }
                var ts = tracks.Where(t => names.Contains(t.Name)).ToList();
                //var draw = new DrawBase(256, 256);
                //draw.OpenAutoFit(
                //    ts.Min(t => t.MinLon),
                //    ts.Min(t => t.MinLat),
                //    ts.Max(t => t.MaxLon),
                //    ts.Max(t => t.MaxLat)
                //    );
                //foreach (var item in ts)
                //{
                //    draw.Draw(item, draw.RandomColor(), 3);
                //}
                //draw.Image.Save($"{imgDir}/{items.Key}_{items.First().Start:yyyy_MM_dd_HH_mm_ss}.png");
            }
        }

        static void TrackClusteringTest(IEnumerable<Trajectory> tracks, ILoggerFactory factory)
        {
            var cluster = new CommonSubsequenceCluster<string>();
            var logger = factory.CreateLogger<CommonSubsequenceCluster<string>>();
            var tracks_tmp = tracks.Select(t => new TrajectoryWithCode<string>().InitFrom(t)).ToList();
            var imgDir = Path.Combine(Directory.GetCurrentDirectory(), "img");
            Parallel.ForEach(tracks_tmp, t =>
                      {
                          //numberOfChars=7
                          t.GeoCodes = t.GeoPoints.Select(tt => GeoHash.Encode(tt.Latitude, tt.Longitude, 7))
                              //t.GeoCodes = t.GeoPoints.Select(tt => GeoHash.EncodeInt(tt.Latitude, tt.Longitude, 35))
                              .Distinct()
                              .ToList();
                      });


            logger.LogInformation(message: tracks.Count().ToString());


            var sh = new Stopwatch();
            sh.Start();
            var tree = cluster.BuildClusterTree(tracks_tmp, 0.7f, 0.4f);
            logger.LogInformation($"BuildClusterTree, count:{tracks.Count()}, time:{sh.Elapsed}");

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