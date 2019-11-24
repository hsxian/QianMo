using System;
using System.Reflection;
using Accord.Math.Geometry;

namespace QianMo.Core.Utilities
{
    public static class GeoLineTool
    {
        public static double GetY(this Line line, double x)
        {
            if (line.IsHorizontal)
            {
                return line.Intercept;
            }

            if (line.IsVertical)
            {
                return double.NaN;
            }

            return line.Slope * x + line.Intercept;
        }

        public static double GetX(this Line line, double y)
        {
            if (line.IsVertical)
            {
                return line.Intercept;
            }

            if (line.IsHorizontal)
            {
                return double.NaN;
            }

            return (y - line.Intercept) / line.Slope;
        }

        /// <summary>
        /// 点到直线的垂直距离
        /// </summary>
        /// <param name="line"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double GeoDistance(this Line line, double x, double y)
        {
            if (line.IsHorizontal)
            {
                return GeoTool.GeoDistance(y, x, y, line.Intercept);
            }
            else if (line.IsVertical)
            {
                return GeoTool.GeoDistance(y, x, line.Intercept, x);
            }

            var h = GeoTool.GeoDistance(y, x, y, line.GetX(y));
            var v = GeoTool.GeoDistance(y, x, line.GetY(x), x);
            return Math.Sqrt(Math.Pow(h, 2) + Math.Pow(v, 2));
        }
    }
}