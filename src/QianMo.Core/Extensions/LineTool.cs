using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace QianMo.Core.Extensions
{
    public class LineTool
    {
    
    }

    class DouglasPeuckerLineSimplifier
    {
        private readonly LineSegment _seg = new LineSegment();
        private readonly Coordinate[] _pts;
        private bool[] _usePt;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="distanceTolerance"></param>
        /// <returns></returns>
        public static Coordinate[] Simplify(Coordinate[] pts, double distanceTolerance)
        {
            return new DouglasPeuckerLineSimplifier(pts)
            {
                DistanceTolerance = distanceTolerance
            }.Simplify();
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="pts" /> array of coordinates
        /// </summary>
        /// <param name="pts">An array of coordinates</param>
        public DouglasPeuckerLineSimplifier(Coordinate[] pts)
        {
            this._pts = pts;
        }

        /// <summary>The distance tolerance for the simplification.</summary>
        public double DistanceTolerance { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Coordinate[] Simplify()
        {
            this._usePt = new bool[this._pts.Length];
            for (var index = 0; index < this._pts.Length; ++index)
                this._usePt[index] = true;
            this.SimplifySection(0, this._pts.Length - 1);
            var coordinateList = new CoordinateList();
            coordinateList.AddRange(this._pts.Where((t, index) => this._usePt[index]).Select(t => t.Copy()));

            return coordinateList.ToCoordinateArray();
        }

        private void SimplifySection(int i, int j)
        {
            if (i + 1 == j)
                return;
            this._seg.P0 = this._pts[i];
            this._seg.P1 = this._pts[j];
            var num1 = -1.0;
            var num2 = i;
            System.Threading.Tasks.Parallel.For(i + 1, j, index => { });
            for (var index = i + 1; index < j; ++index)
            {
                var num3 = this._seg.Distance(this._pts[index]);
                if (num3 > num1)
                {
                    num1 = num3;
                    num2 = index;
                }
            }

            if (num1 <= this.DistanceTolerance)
            {
                for (var index = i + 1; index < j; ++index)
                    this._usePt[index] = false;
            }
            else
            {
                this.SimplifySection(i, num2);
                this.SimplifySection(num2, j);
            }
        }
    }
}