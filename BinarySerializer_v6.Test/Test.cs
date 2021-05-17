using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BinarySerializer_v5.Test
{
    public delegate void TestDelegate(Stopwatch sw);

    public class Test
    {
        private List<double> TimeTable = new List<double>();

        private int iteration;

        private string testName;

        private double firstIteration;

        public void Run(int iteration, string testName, TestDelegate func)
        {
            TimeTable.Clear();

            this.iteration = iteration;
            this.testName = testName;

            Stopwatch sw = new Stopwatch();


            sw.Reset();

            func(sw);

            firstIteration = sw.Elapsed.TotalMilliseconds;

            for (int i = 0; i < iteration; i++)
            {
                sw.Reset();
                func(sw);

                TimeTable.Add(sw.Elapsed.TotalMilliseconds);
            }

            ShowTestData();
        }

        public void ShowTestData()
        {
            Console.WriteLine($"Test - {testName}, Iterations - {iteration}, First result - {firstIteration} ms, Min value {TimeTable.Min()} ms, Max value {TimeTable.Max()} ms, Average value {TimeTable.Average()} ms, Total value {TimeTable.Sum()} ms");
        }
    }
}
