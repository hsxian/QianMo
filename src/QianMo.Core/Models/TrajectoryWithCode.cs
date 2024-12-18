using System.Collections.Concurrent;
using System.Collections.Generic;

namespace QianMo.Core.Models
{
        public class TrajectoryWithCode<TGeoCode> : Trajectory
        {
                public List<TGeoCode> GeoCodes { get; set; }
                public TrajectoryWithCode<TGeoCode> Parent { get; set; }
                public ConcurrentBag<TrajectoryWithCode<TGeoCode>> Siblings { get; } = new ConcurrentBag<TrajectoryWithCode<TGeoCode>>();
                public ConcurrentBag<TrajectoryWithCode<TGeoCode>> Children { get; } = new ConcurrentBag<TrajectoryWithCode<TGeoCode>>();
                public TrajectoryWithCode<TGeoCode> InitFrom(Trajectory t)
                {
                        Id = t.Id;
                        MaxLat = t.MaxLat;
                        MaxLon = t.MaxLon;
                        MinLat = t.MinLat;
                        MinLon = t.MinLon;
                        GeoPoints = t.GeoPoints;
                        Name = t.Name;
                        Start = t.Start;
                        End = t.End;
                        Level = t.Level;
                        LevelTag = t.LevelTag;
                        return this;
                }
        }

}