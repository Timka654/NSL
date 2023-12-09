using NSL.Generators.FillTypeGenerator.Tests.Develop.WithModelName;

namespace NSL.Generators.FillTypeGenerator.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {

            List<WithModelName1> aa = new List<WithModelName1>();

            var t = aa.Selectabc2().FirstOrDefault();
            var t2 = aa.Selectabc1();


            List<WithModelName4> bb = new List<WithModelName4>();

            bb.Selectmodel1().FirstOrDefault();
            bb.Selectmodel2().FirstOrDefault();
            bb.Selectmodel3().FirstOrDefault();


#if !DEVELOP
            Test1Model tmodel1 = new Test1Model() { TestValue1 = "abcxaca", TestValue2 = "35t2452" };

            Test2Model tmodel2 = new Test2Model() { };

            tmodel1.FillTo(tmodel2);
#endif
        }
    }
}