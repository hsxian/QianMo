using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QianMo.Core.Infrastructure;
using QianMo.Core.Models;

namespace QianMo.ConsoleApp
{
    public class DataFileReader
    {
        public IEnumerable<string> GetDataFiles(string path, string searchPattern = "*")
        {
            if (Directory.Exists(path) == false) return null;
            var files = Directory.GetFiles(path, searchPattern).ToList();
            var paths = Directory.GetDirectories(path);
            paths.ToList().ForEach(t =>
            {
                var subFiles = GetDataFiles(t, searchPattern);
                if (subFiles?.Any() == true)
                {
                    files.AddRange(subFiles);
                }
            });
            return files;
        }

        public IEnumerable<Trajectory> GetTrajectories(IEnumerable<string> files)
        {
            return files
                .Where(File.Exists)
                .Select(file => GetTrajectory(file, digits: 3))
                .Where(track => track.GeoPoints.Any())
                .ToList();
        }

        private Trajectory GetTrajectory(string file, int startLine = 6, int digits = 2)
        {
            var result = new Trajectory
            {
                Name = Path.GetFileName(file),
                GeoPoints = new List<IGeoInfoModel>()
            };
            var lineCount = 0;
            using (var fs = new StreamReader(file))
            {
                while (fs.EndOfStream == false)
                {
                    var line = fs.ReadLine();
                    if (lineCount++ < startLine) continue;
                    var arr = line.Split(",");
                    var point = new GeoInfoModel
                    {
                        Latitude = float.Parse(arr[0]),
                        Longitude = float.Parse(arr[1]),
                        Time = DateTime.Parse($"{arr[5]} {arr[6]}")
                    };
                    result.GeoPoints.Add(point);
                }
            }
            return result;
        }
    }
}