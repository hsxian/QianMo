using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using QianMo.Core.Models;

namespace QianMo.Core.Infrastructure
{
    public class GeoHelper
    {
        public static IEnumerable<string> GetGeoCodes(IEnumerable<IGeoInfoModel> geoData, int digits = 2)
        {
            var dig = $"F{digits}";
            return geoData
                .Select(t =>
                    $"{t.Latitude.ToString(dig)}_{t.Longitude.ToString(dig)}")
                .Distinct().ToList();
        }
    }
}