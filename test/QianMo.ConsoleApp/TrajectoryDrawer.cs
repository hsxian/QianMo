using System;
using System.Drawing;
using System.Linq;
using QianMo.Core.Models;

namespace QianMo.ConsoleApp
{
    public class TrajectoryDrawer
    {
        private readonly int _w;
        private readonly int _h;

        public TrajectoryDrawer(int w, int h)
        {
            _w = w;
            _h = h;
        }

        private float GetScaleOfBitmap(float minLat, float minLon, float maxLat, float maxLon)
        {
            var diffLat = maxLat - minLat;
            var diffLon = maxLon - minLon;
            var scaleLat = _h / diffLat;
            var scaleLon = _w / diffLon;
            var scale = Math.Min(scaleLat, scaleLon);
            return scale;
        }

        public Bitmap CreatBitmap(Trajectory trajectory, float minLat, float minLon, float maxLat, float maxLon)
        {
            var s = GetScaleOfBitmap(minLat, minLon, maxLat, maxLon);
            var bitmap = new Bitmap(_w, _h);
            var draw = Graphics.FromImage(bitmap);
            var points = trajectory.GeoPoints.ToList().Select(t =>
            {
                var h = t.Latitude - minLat;
                h *= s;
                var w = t.Longitude - minLon;
                w *= s;
                return new PointF(w, h);
            }).ToArray();
            draw.DrawLines(new Pen(Color.Red, 3), points);
            return bitmap;
        }

        public Bitmap CreatBitmap(Trajectory trajectory)
        {
            return CreatBitmap(trajectory, trajectory.MinLat, trajectory.MinLon, trajectory.MaxLat, trajectory.MaxLon);
        }
    }
}