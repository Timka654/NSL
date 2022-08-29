using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NSL.Extensions.NetScript;

namespace NSL.Extensions.NetScript.Example
{
    class Program
    {
        static Dictionary<int, TScript> scripts = new Dictionary<int, TScript>();
        static void Main(string[] args)
        {
            //try
            //{
            //    throw new Exception("abc");
            //}
            //catch (Exception ex)
            //{
            //    string test = ex.ToString();
            //}

            scripts.Add(0, new TScript(0));
            scripts.Add(1, new TScript(1));
            Random r = new Random();
            for (int i = 0; i < 21; i++)
            {
                scripts[r.Next(0, 2)].TestCall();
            }
            scripts[0].ShowCallCount();
            scripts[1].ShowCallCount();
            Console.ReadKey();
        }
    }
}
