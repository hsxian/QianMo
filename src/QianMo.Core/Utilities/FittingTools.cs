using System;
using System.Collections.Generic;
using System.Linq;
using Accord;
using Accord.MachineLearning.Geometry;
using Accord.Statistics.Models.Regression.Linear;
using QianMo.Core.Models;

namespace QianMo.Core.Utilities
{
    public class FittingTools
    {
        public CircleModel FittingCircleWithLeastSquares(double[][] points)
        {
            var ols = new OrdinaryLeastSquares
            {
                UseIntercept = true,
                IsRobust = true
            };
            var outputs = points.Select(t => Math.Pow(t[0], 2) + Math.Pow(t[1], 2)).ToArray();
            var regression = ols.Learn(points, outputs);

            // As result, we will be given the following:
            var a = regression.Weights[0] / 2; // a = 0
            var b = regression.Weights[1] / 2; // b = 0
            var c = regression.Intercept; // c = 1
            c = Math.Sqrt(c + a * a + b * b);

            var midPoint = points[points.Length / 2];
            var result = new CircleModel
            {
                X = a,
                Y = b,
                XSource = midPoint[0],
                YSource = midPoint[1],
                R = c,
                R_geo = GeoTools.CalculateDistance(midPoint[1], midPoint[0], b, a)
            };
            return result;
        }
        
        public CircleModel FittingCircleWithRansac(double[][] points)
        {
            var ransca = new RansacCircle(100, 0.9);

            var circle = ransca.Estimate(points.Select(t => new Point((float) t[0], (float) t[1])).ToArray());
            var midPoint = points[points.Length / 2];
            var result = new CircleModel
            {
                X = circle.Origin.X,
                Y = circle.Origin.Y,
                XSource = midPoint[0],
                YSource = midPoint[1],
                R = circle.Radius,
                R_geo = GeoTools.CalculateDistance(midPoint[1], midPoint[0], circle.Origin.Y, circle.Origin.X)
            };
            return result;
        }
    }
}