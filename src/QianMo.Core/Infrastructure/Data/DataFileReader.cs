using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Accord.Math.Geometry;
using QianMo.Core.Models;

namespace QianMo.Core.Infrastructure.Data
{
    public class DataFileReader<TGeoPoint> where TGeoPoint : IGeoInfoModel, new()
    {
        public IList<Trajectory> GetTrajectories(IEnumerable<string> files)
        {
            return files
                .Where(File.Exists)
                .Select(file => GetTrajectory(file))
                .Where(track => track.GeoPoints.Any())
                .ToList();
        }
        public static Trajectory GetTrajectory(string name, string fileContent, string separator = "\r\n", int startLine = 6)
        {
            var lines = fileContent.Split(separator).Skip(startLine).ToList();
            if (lines.Any()) lines.RemoveAt(lines.Count - 1);
            var ps = lines.Select(line =>
              {
                  var arr = line.Split(',');
                  var point = new TGeoPoint
                  {
                      Latitude = float.Parse(arr[0]),
                      Longitude = float.Parse(arr[1]),
                      Time = DateTime.Parse($"{arr[5]} {arr[6]}")
                  };
                  return point;
              }).OrderBy(t => t.Time).ToList();
            if (ps.Count == 0) return null;
            var result = new Trajectory
            {
                Name = name
            };
            result.Init(ps.Cast<IGeoInfoModel>().ToList());
            return result;
        }
        public static Trajectory GetTrajectory(string file, int startLine = 6)
        {
            var result = new Trajectory
            {
                Name = Path.GetFileName(file)
            };
            var lineCount = 0;
            var ps = new List<IGeoInfoModel>();
            using (var fs = new StreamReader(file))
            {
                while (fs.EndOfStream == false)
                {
                    var line = fs.ReadLine();
                    if (lineCount++ < startLine) continue;
                    var arr = line.Split(',');
                    var point = new TGeoPoint
                    {
                        Latitude = float.Parse(arr[0]),
                        Longitude = float.Parse(arr[1]),
                        Time = DateTime.Parse($"{arr[5]} {arr[6]}")
                    };
                    ps.Add(point);
                }
            }
            result.Init(ps.OrderBy(t => t.Time).ToList());
            return result;
        }
    }
}