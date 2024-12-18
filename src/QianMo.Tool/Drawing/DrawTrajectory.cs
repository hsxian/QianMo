using System.Linq;
using QianMo.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace QianMo.Tool.Drawing
{
    public static class DrawTrajectory
    {
        public static DrawBase Draw(this DrawBase drawer, Trajectory trajectory, Color color, float width)
        {
            var points = trajectory.GeoPoints.ToList().Select(t =>
            {
                if (drawer.AutoFitScale == 1) return new PointF(t.Longitude, t.Latitude);
                var h = t.Latitude - drawer.MinY;
                h *= drawer.AutoFitScale;
                var w = t.Longitude - drawer.MinX;
                w *= drawer.AutoFitScale;
                return new PointF(w, h);

            }).ToArray();
            drawer.Image.Mutate(t =>
            {
                t.DrawLine(color, width, points);
            });
            // draw.DrawLines(new SixLabors.ImageSharp.Drawing.ProcessingPen(color, width), points);
            return drawer;
        }
    }
}