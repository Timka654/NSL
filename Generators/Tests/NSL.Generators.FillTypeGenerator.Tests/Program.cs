using NSL.Generators.FillTypeGenerator.Tests.Develop.NoEqMemberTypeWithEqNames;
using NSL.Generators.FillTypeGenerator.Tests.From;

namespace NSL.Generators.FillTypeGenerator.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEVELOP
            DevClass1 devClass1 = new DevClass1();

            DevClass2 devClass2 = new DevClass2();

            devClass1.FillFrom(devClass2);
#else

            Test6Model model5 = new Test6Model();
            Test6Model model6 = new Test6Model();
            Test7Model model7 = new Test7Model();

            model7.Filla1To(model6);
            model7.FillTo(model6);

            model7.FillFrom(model6);
#endif
        }
    }
}