using System.Drawing;
using System.Linq;
using QianMo.Core.Models;

namespace QianMo.Core.Infrastructure.Drawing
{
    public static class DrawTrajectory
    {
        public static DrawBase Draw(this DrawBase drawer, Trajectory trajectory, Color color, float width)
        {
            var draw = Graphics.FromImage(drawer.Image);
            var points = trajectory.GeoPoints.ToList().Select(t =>
            {
                if (drawer.AutoFitScale == 1) return new PointF(t.Longitude, t.Latitude);
                var h = t.Latitude - drawer.MinY;
                h *= drawer.AutoFitScale;
                var w = t.Longitude - drawer.MinX;
                w *= drawer.AutoFitScale;
                return new PointF(w, h);

            }).ToArray();
            draw.DrawLines(new Pen(color, width), points);
            return drawer;
        }
    }
}