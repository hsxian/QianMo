using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using QianMo.Core.Models;
using QianMo.Core.Utilities;

namespace QianMo.Core.Algorithms
{
    public class SpatioTemporalCoexistence
    {
        private readonly ILogger _logger;
        public SpatioTemporalCoexistence(ILogger logger)
        {
            _logger = logger;

        }
        public double CalculateDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        public (List<IGeoInfoModel>, List<IGeoInfoModel>)? CheckForBoxIntersection(
        Trajectory tmi,
        Trajectory tmj,
        TimeSpan timeThreshold)
        {
            bool timeBool = tmi.End > tmj.Start && tmi.Start < tmj.End;
            if (!timeBool)
            {
                return null;
            }

            DateTime start = ComparableTool.Max(tmi.Start, tmj.Start);
            DateTime end = ComparableTool.Min(tmi.End, tmj.End);
            if ((end - start) < timeThreshold)
            {
                return null;
            }

            _logger.LogDebug("start:{0}, end:{1}, span:{2}", start, end, end - start);

            var tracki = tmi.GeoPoints.Where(p => p.Time >= start && p.Time <= end).ToList();
            var trackj = tmj.GeoPoints.Where(p => p.Time >= start && p.Time <= end).ToList();

            if (tracki.Count == 0 || trackj.Count == 0) return null;
            double minxi = tracki.Min(p => p.Longitude);
            double minyi = tracki.Min(p => p.Latitude);
            double maxxi = tracki.Max(p => p.Longitude);
            double maxyi = tracki.Max(p => p.Latitude);

            double minxj = trackj.Min(p => p.Longitude);
            double minyj = trackj.Min(p => p.Latitude);
            double maxxj = trackj.Max(p => p.Longitude);
            double maxyj = trackj.Max(p => p.Latitude);

            bool xyBool = maxxi > minxj && minxi < maxxj && maxyi > minyj && minyi < maxyj;
            if (xyBool)
            {
                return (tracki, trackj);
            }

            return null;
        }
        public List<Tuple<DateTime, DateTime>> MergeTimes(List<Tuple<DateTime, DateTime>> times, int timeThresholdSecs)
        {
            var ret = new List<Tuple<DateTime, DateTime>>();
            if (times.Count == 0) return ret;
            ret.Add(times[0]);
            foreach (var (startTime, endTime) in times)
            {
                var et = ret[^1].Item2;
                if (et < endTime)
                {
                    var timeDiff = (et - startTime).TotalSeconds;
                    if (Math.Abs(timeDiff) < timeThresholdSecs)
                    {
                        ret[^1] = Tuple.Create(ret[^1].Item1, endTime);
                    }
                    else
                    {
                        ret.Add(Tuple.Create(startTime, endTime));
                    }
                }
            }
            return ret;
        }

        public List<Tuple<DateTime, DateTime>> FindCompanionSegments(
            List<IGeoInfoModel> df1,
            List<IGeoInfoModel> df2,
            TimeSpan timeThreshold,
            double distanceThreshold,
            int step = 1)
        {
            int i = 0, j = 0;
            int len1 = df1.Count, len2 = df2.Count;
            len1 -= step;
            len2 -= step;
            double timeThresholdSecs = timeThreshold.TotalSeconds;
            var times = new List<Tuple<DateTime, DateTime>>();

            while (i < len1 && j < len2)
            {
                var dfi = df1[i];
                var dfj = df2[j];
                var time1 = dfi.Time;
                var lon1 = dfi.Longitude;
                var lat1 = dfi.Latitude;
                var time2 = dfj.Time;
                var lon2 = dfj.Longitude;
                var lat2 = dfj.Latitude;
                var startTime = new DateTime(Math.Max(time1.Ticks, time2.Ticks));

                if (Math.Abs((time1 - time2).TotalSeconds) <= timeThresholdSecs)
                {
                    double distance = CalculateDistance(lat1, lon1, lat2, lon2);

                    if (distance < distanceThreshold)
                    {
                        int k1 = i + step, k2 = j + step;
                        var dfik = df1[k1];
                        var dfjk = df2[k2];
                        while (k1 < len1 && k2 < len2 &&
                               Math.Abs((dfik.Time - dfjk.Time).TotalSeconds) <= timeThresholdSecs &&
                               // 再次假设距离在阈值内
                               CalculateDistance(dfik.Longitude, dfik.Latitude, dfjk.Longitude, dfjk.Latitude) < distanceThreshold)
                        {
                            i = k1;
                            j = k2;
                            k1 += step;
                            k2 += step;
                            dfik = df1[k1];
                            dfjk = df2[k2];
                        }
                        var endTime = new DateTime(Math.Min(df1[i].Time.Ticks, df2[j].Time.Ticks));
                        if (startTime < endTime)
                            times.Add(Tuple.Create(startTime, endTime));
                    }
                }

                if (time1 < time2 || (time1 == time2 && i < j))
                {
                    i += step;
                }
                else
                {
                    j += step;
                }
            }
            return times.Count == 0 ? null : times;
        }
        public void GenerateGroupName(IList<TwoTrajectoryGroup> twoItemList)
        {
            var hasIdxs = new HashSet<int>();
            for (int i = 0; i < twoItemList.Count; i++)
            {
                if (hasIdxs.Contains(i)) continue;
                var new_group = new HashSet<int> { i };
                hasIdxs.Add(i);
                var current = twoItemList[i];
                for (int j = i + 1; j < twoItemList.Count; j++)
                {
                    if (hasIdxs.Contains(j)) continue;
                    var other = twoItemList[j];
                    //检查 Name 和时间范围的交集
                    if ((
                        current.Name1 == other.Name1
                        || current.Name1 == other.Name2
                        || current.Name2 == other.Name1
                        || current.Name2 == other.Name2
                        )
                        && other.End > current.Start && other.Start < current.End)
                    {

                        // 如果有交集，将其他组的索引添加到新组中
                        new_group.Add(j);
                        hasIdxs.Add(j);
                    }
                }
                var group = string.Join('_', new_group.Select(t => t.ToString()));
                foreach (var idx in new_group)
                {
                    twoItemList[idx].Group = group;
                }
                if (new_group.Count > 1)
                {
                    _logger.LogDebug("group name: {0}", group);
                }
            }
        }
    }
}