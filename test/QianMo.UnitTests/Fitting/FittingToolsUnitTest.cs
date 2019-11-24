using System;
using System.Linq;
using QianMo.Core.Utilities;
using Xunit;

namespace QianMo.UnitTests.Fitting
{
    public class FittingToolsUnitTest
    {
        private const double X0 = 3.8;
        private const double Y0 = -50.8;
        private const double R = 123.8;
        private const int PointCount = 100;
        private readonly double[][] Points;
        private readonly double[] CircleResult;

        public FittingToolsUnitTest()
        {
            const double step = Math.PI * 2.0 / PointCount;
            const double offset = 0.01;

            Points = new double[PointCount][];
            CircleResult = new double[PointCount];


            var rd = new Random();
            var angle = 0.0;
            for (var i = 0; i < PointCount; i++)
            {
                Points[i] = new[]
                {
                    X0 + R * Math.Cos(angle) + rd.NextDouble() * offset * Math.Cos(Math.PI * rd.Next()),
                    Y0 + R * Math.Sin(angle) + rd.NextDouble() * offset * Math.Cos(Math.PI * rd.Next())
                };
                angle += step;
                CircleResult[i] = Math.Pow(Points[i][0], 2) + Math.Pow(Points[i][1], 2);
            }
        }

        [Fact]
        public void TestFittingCircleWithLeastSquaresWhereMajorArc()
        {
            var fitting = new FittingTool();
            const int takeCount = (int) (PointCount * 0.6);
            var circle = fitting.FittingCircleWithLeastSquares(Points.Take(takeCount).ToArray());
            Assert.Equal(circle.X, X0, 1);
            Assert.Equal(circle.Y, Y0, 1);
            Assert.Equal(circle.R, R, 1);
        }

        [Fact]
        public void TestFittingCircleWithLeastSquaresWhereMinorArc()
        {
            var fitting = new FittingTool();
            var takeCount = (int) (PointCount * 0.1);
            takeCount = takeCount < 3 ? 3 : takeCount;
            var circle = fitting.FittingCircleWithLeastSquares(Points.Take(takeCount).ToArray());
            Assert.Equal(circle.X, X0, 0);
            Assert.Equal(circle.Y, Y0, 0);
            Assert.Equal(circle.R, R, 0);
        }

        [Fact]
        public void TestFittingCircleWithRansacWhereMajorArc()
        {
            var fitting = new FittingTool();
            const int takeCount = (int) (PointCount * 0.6);
            var circle = fitting.FittingCircleWithRansac(Points.Take(takeCount).ToArray());
            Assert.Equal(circle.X, X0, 1);
            Assert.Equal(circle.Y, Y0, 1);
            Assert.Equal(circle.R, R, 1);
        }

        [Fact]
        public void TestFittingCircleWithRansacWhereMinorArc()
        {
            var fitting = new FittingTool();
            var takeCount = (int) (PointCount * 0.1);
            takeCount = takeCount < 3 ? 3 : takeCount;
            var circle = fitting.FittingCircleWithRansac(Points.Take(takeCount).ToArray());
            Assert.Equal(circle.X, X0, 0);
            Assert.Equal(circle.Y, Y0, 0);
            Assert.Equal(circle.R, R, 0);
        }
    }
}