using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QianMo.Core.Utilities
{
    public class ComparableTool
    {
        public static T Min<T>(T t1, T t2) where T : IComparable
        {
            return t1.CompareTo(t2) < 0 ? t1 : t2;
        }
        public static T Max<T>(T t1, T t2) where T : IComparable
        {
            return t1.CompareTo(t2) < 0 ? t2 : t1;
        }
    }
}