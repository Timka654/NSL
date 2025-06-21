
using NSL.Generators.FillTypeGenerator.Tests.Develop;

namespace NSL.Generators.FillTypeGenerator.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEVELOP
            var dup_val1 = new DuplicateNameModel() { Id = "val1" };
            var dup_val2 = new DuplicateNameModel<int>() { Id = "val2" };

            var dup_valTo = new DuplicateNameToModel();

            dup_val1.FillTo(dup_valTo);
            dup_val2.FillTo(dup_valTo);
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

            Test1Model tmodel1 = new Test1Model() { TestValue1 = "abcxaca", TestValue2 = "35t2452" };

            Test2Model tmodel2 = new Test2Model() { };

            tmodel1.FillTo(tmodel2);

            var p = new ProxyModel1();
            new ProxyModel1()
            {
                M2List = new List<ProxyModel2>() {
                    new ProxyModel2() { a1 = 1, q = "q1" },
                    new ProxyModel2() { a1 = 2, q = "q2" },
                    new ProxyModel2() { a1 = 3, q = "q3" },
                }
            }.FillInstanceUpdateTo(p);
#endif
        }
    }
}