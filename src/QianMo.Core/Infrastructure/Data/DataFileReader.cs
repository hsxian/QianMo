using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QianMo.Core.Models;

namespace QianMo.Core.Infrastructure.Data
{
    public class DataFileReader<TGeoPoint> where TGeoPoint : IGeoInfoModel, new()
    {
        public IEnumerable<Trajectory<TGeoCode>> GetTrajectories<TGeoCode>(IEnumerable<string> files)
        {
            return files
                .Where(File.Exists)
                .Select(file => GetTrajectory<TGeoCode>(file, digits: 3))
                .Where(track => (track).GeoPoints.Any())
                .ToList();
        }

        private  Trajectory<TGeoCode> GetTrajectory<TGeoCode>(string file, int startLine = 6, int digits = 2)
        {
            var result = new Trajectory<TGeoCode>
            {
                Name = Path.GetFileName(file)
            };
            var lineCount = 0;
            var ps = new List<IGeoInfoModel>();
            using (var fs = new StreamReader(file))
            {
                while (fs.EndOfStream == false)
                {
                    var line =  fs.ReadLine();
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
            result.Init(ps);
            return result;
        }
    }
}