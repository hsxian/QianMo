using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace QianMo.Core.Models
{
    public class Trajectory
    {
        public Guid Id { get; protected set; }

        public Trajectory()
        {
            Id = Guid.NewGuid();
        }

        public string Name { get; set; }
        public List<IGeoInfoModel> GeoPoints { get; protected set; }
        public void Init(List<IGeoInfoModel> ps)
        {
            GeoPoints = ps;

            MinLat = GeoPoints.Min(t => t.Latitude);
            MinLon = GeoPoints.Min(t => t.Longitude);
            MaxLat = GeoPoints.Max(t => t.Latitude);
            MaxLon = GeoPoints.Max(t => t.Longitude);
            Start = GeoPoints.First().Time;
            End = GeoPoints.Last().Time;

        }
        public float MinLat { get; protected set; }
        public float MinLon { get; protected set; }
        public float MaxLat { get; protected set; }
        public float MaxLon { get; protected set; }
        public DateTime Start { get; protected set; }
        public DateTime End { get; protected set; }

        public bool DoRectanglesIntersect(Trajectory trajectory)
        {
            bool hasHorizontalIntersection = MinLon < trajectory.MaxLon && MaxLon > trajectory.MinLon;
            bool hasVerticalIntersection = MinLat < trajectory.MaxLat && MaxLat > trajectory.MinLat;

            return hasHorizontalIntersection && hasVerticalIntersection;
        }
        public int Level { get; set; }
        public string LevelTag { get; set; }
    }
}