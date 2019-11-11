using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace QianMo.Core.Models
{
    public class Trajectory
    {
        public Guid Id { get; }

        public Trajectory()
        {
            Id = Guid.NewGuid();
        }

        public string Name { get; set; }
        public List<IGeoInfoModel> GeoPoints { get; set; }
        public List<string> GeoCodes { get; set; }
        public Trajectory Parent { get; set; }
        public ConcurrentBag<Trajectory> Siblings { get; } = new ConcurrentBag<Trajectory>();
        public ConcurrentBag<Trajectory> Children { get; } = new ConcurrentBag<Trajectory>();

        public float MinLat => GeoPoints.Min(t => t.Latitude);
        public float MinLon => GeoPoints.Min(t => t.Longitude);
        public float MaxLat => GeoPoints.Max(t => t.Latitude);
        public float MaxLon => GeoPoints.Max(t => t.Longitude);

        public int Level { get; set; }
        public string LevelTag { get; set; }
    }
}