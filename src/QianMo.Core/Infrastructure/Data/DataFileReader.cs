using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QianMo.Core.Models;

namespace QianMo.Core.Infrastructure.Data
{
    public class DataFileReader<TGeoPoint> where TGeoPoint : IGeoInfoModel, new()
    {
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
                    var arr = line.Split(',');
                    var point = new TGeoPoint
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