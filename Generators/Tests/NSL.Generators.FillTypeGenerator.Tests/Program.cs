﻿namespace NSL.Generators.FillTypeGenerator.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if !DEVELOP
            Test1Model tmodel1 = new Test1Model() { TestValue1 = "abcxaca", TestValue2 = "35t2452" };

            Test2Model tmodel2 = new Test2Model() { };

            tmodel1.FillTo(tmodel2);
#endif
        }
    }
}