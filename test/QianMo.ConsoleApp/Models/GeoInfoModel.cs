using System;
using QianMo.Core.Models;

namespace QianMo.ConsoleApp.Models
{
    public class GeoInfoModel:IGeoInfoModel
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float Altitude { get; set; }
        public DateTime Time { get; set; }
    }
}