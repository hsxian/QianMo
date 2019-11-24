using System;
using System.Collections.Generic;
using System.Linq;
using QianMo.Core.Models;

namespace QianMo.Core.Utilities
{
    public class GeoTool
    {
        public static IEnumerable<string> GetGeoCodes(IEnumerable<IGeoInfoModel> geoData, int digits = 2)
        {
            var dig = $"F{digits}";
            return geoData
                .Select(t =>
                    $"{t.Latitude.ToString(dig)}_{t.Longitude.ToString(dig)}")
                .Distinct().ToList();
        }

        public static double GeoDistance(double sLatitude, double sLongitude, double eLatitude, double eLongitude)
        {
            const double radiansOverDegrees = Math.PI / 180.0;
            // 地球半径，单位：米
            const double diameterOfEarth = 6371000.0 * 2.0;

            var sLatitudeRadians = sLatitude * radiansOverDegrees;
            var sLongitudeRadians = sLongitude * radiansOverDegrees;
            var eLatitudeRadians = eLatitude * radiansOverDegrees;
            var eLongitudeRadians = eLongitude * radiansOverDegrees;

            var dLongitude = eLongitudeRadians - sLongitudeRadians;
            var dLatitude = eLatitudeRadians - sLatitudeRadians;

            var result1 = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                          Math.Cos(sLatitudeRadians) * Math.Cos(eLatitudeRadians) *
                          Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            var result2 = diameterOfEarth * Math.Atan2(Math.Sqrt(result1), Math.Sqrt(1.0 - result1));

            return result2;
        }
    }
}