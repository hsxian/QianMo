﻿using System;

namespace QianMo.Core.Models
{
    public class GeoInfoModel : IGeoInfoModel
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float Altitude { get; set; }
        public DateTime Time { get; set; }

        public override string ToString()
        {
            return $"{Time:yyyy-MM-dd HH:mm:ss}:{Longitude},{Latitude},{Altitude}";
        }
    }
}