using System;

namespace QianMo.Core.Models
{
    public interface IGeoInfoModel
    {
        float Latitude { get; set; }
        float Longitude { get; set; }
        float Altitude { get; set; }
        DateTime Time { get; set; }
    }
}