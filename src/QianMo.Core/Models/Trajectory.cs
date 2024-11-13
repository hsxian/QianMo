using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace QianMo.Core.Models
{
    public class Trajectory<TGeoCode>
    {
        public Guid Id { get; }

        public Trajectory()
        {
            Id = Guid.NewGuid();
        }

        public string Name { get; set; }
        public List<IGeoInfoModel> GeoPoints { get; private set; }
        public List<TGeoCode> GeoCodes { get; set; }
        public Trajectory<TGeoCode> Parent { get; set; }
        public ConcurrentBag<Trajectory<TGeoCode>> Siblings { get; } = new ConcurrentBag<Trajectory<TGeoCode>>();
        public ConcurrentBag<Trajectory<TGeoCode>> Children { get; } = new ConcurrentBag<Trajectory<TGeoCode>>();
        public void Init(List<IGeoInfoModel> ps)
        {
            GeoPoints = ps;

            MinLat = GeoPoints.Min(t => t.Latitude);
            MinLon = GeoPoints.Min(t => t.Longitude);
            MaxLat = GeoPoints.Max(t => t.Latitude);
            MaxLon = GeoPoints.Max(t => t.Longitude);

        }
        public float MinLat { get; private set; }
        public float MinLon { get; private set; }
        public float MaxLat { get; private set; }
        public float MaxLon { get; private set; }

        public bool DoRectanglesIntersect(Trajectory<TGeoCode> trajectory)
        {
            // 检查两个矩形在水平和垂直方向上是否有交集
            bool hasHorizontalIntersection = MinLon < trajectory.MaxLon && MaxLon > trajectory.MinLon;
            bool hasVerticalIntersection = MinLat < trajectory.MaxLat && MaxLat > trajectory.MinLat;

            // 如果两个方向都有交集，则返回true，否则返回false
            return hasHorizontalIntersection && hasVerticalIntersection;
        }
        public int Level { get; set; }
        public string LevelTag { get; set; }
    }
}