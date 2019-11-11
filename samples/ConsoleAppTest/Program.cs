using System;
using System.Linq;
using System.Numerics;
using Accord.Math;
using Accord.Math.Transforms;

namespace ConsoleAppTest
{
    class Program
    {
        static void Main(string[] args)
        {
//            Console.WriteLine("Hello World!");
            const int end = 100;
//            const int step = 1;
//            var count = end / step;
//            if (end % step != 0) count++;
//            var data = new Complex [count];
//            var j = 0;
//            for (var i = 0; i < end; i += step)
//            {
//                data[j++] = new Complex(i, 1);
//            }
//
//
////            data = Enumerable.Range(0, count).Select(t => new Complex(t, 1)).ToArray();
//            data[^3..]
//                .ToList()
//                .ForEach(t =>
//                    Console.WriteLine($"{t},{t.Real / t.Imaginary}")
//                );
//            Console.WriteLine($"count:{count}, end:{end}, step:{step}");
//            FourierTransform2.FFT(data, FourierTransform.Direction.Forward);
//            var result = data.Where(t => t.Imaginary > 0).ToList();
//            Console.WriteLine($"result count:{count}");
//            foreach (var t in result)
//            {
//                Console.WriteLine($"{t}");
//            }

            var data2 = new Complex[end];
            data2[0] = new Complex(0, 1);
            for (var i = 1; i < end; i++)
            {
                data2[i] = new Complex(i, 0);
                if (i % 4 == 0)
                {
                    data2[i] = new Complex(i, 1);
                }
            }

            FourierTransform2.FFT(data2, FourierTransform.Direction.Forward);
            var result = data2.Take(data2.Length/2).ToList();
            for (var i = 0; i < result.Count; i++)
            {
                Console.WriteLine($"period:{i},m:{result[i].Magnitude:f2},{result[i]},{result[i].Phase}");
            }
        }
    }
}