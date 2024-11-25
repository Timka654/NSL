using NSL.Generators.FillTypeGenerator.Tests.Develop.Proxy;

namespace NSL.Generators.FillTypeGenerator.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEVELOP
            var p = new ProxyModel1();
            new ProxyModel1()
            {
                M2List = new List<ProxyModel2>() {
                    new ProxyModel2() { a1 = 1, q = "q1" },
                    new ProxyModel2() { a1 = 2, q = "q2" },
                    new ProxyModel2() { a1 = 3, q = "q3" },
                }
            }.FillInstanceUpdateTo(p);
#else
            DevClass1 devClass1 = new DevClass1();

            DevClass2 devClass2 = new DevClass2();

            devClass1.FillFrom(devClass2);

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