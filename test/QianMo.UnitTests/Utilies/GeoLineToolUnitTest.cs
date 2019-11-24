using System;
using Accord;
using Accord.Math.Geometry;
using QianMo.Core.Utilities;
using Xunit;

namespace QianMo.UnitTests.Utilies
{
    public class GeoLineToolUnitTest
    {
        private readonly Line vLine = Line.FromPoints(new Point(12, 0), new Point(12, 13));
        private readonly Line hLine = Line.FromPoints(new Point(0, 12), new Point(13, 12));
        private readonly Line line = Line.FromPoints(new Point(), new Point(1, 2));


        [Fact]
        public void TestGetXYFormLine()
        {
            var rd = new Random();
            Assert.Equal(12, vLine.GetX(rd.Next()), 0);
            Assert.Equal(double.NaN, vLine.GetY(rd.Next()));

            Assert.Equal(double.NaN, hLine.GetX(rd.Next()));
            Assert.Equal(12, hLine.GetY(rd.Next()), 0);

            var i = rd.Next(int.MaxValue / 2);
            Assert.Equal(i / 2.0, line.GetX(i), 0);
            Assert.Equal(i * 2.0, line.GetY(i), 0);
        }

        [Fact]
        public void TestGeoDistanceFromLine()
        {
            var rd = new Random();
            Assert.Equal(
                GeoTool.GeoDistance(10, 12, 10, 10),
                hLine.GeoDistance(10, 10));

            Assert.Equal(
                GeoTool.GeoDistance(12, 10, 10, 10),
                vLine.GeoDistance(10, 10));


            Assert.Equal(0, line.GeoDistance(10, 20));
            Assert.Equal(
                Math.Sqrt(
                    Math.Pow(GeoTool.GeoDistance(20, 0, 0, 0), 2.0)
                    +
                    Math.Pow(GeoTool.GeoDistance(0, 10, 0, 0), 2.0)
                ),
                line.GeoDistance(10, 0));
        }
    }
}