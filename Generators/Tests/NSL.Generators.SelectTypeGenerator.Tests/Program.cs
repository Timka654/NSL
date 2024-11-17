

using NSL.Generators.SelectTypeGenerator.Tests.Develop;

namespace NSL.Generators.FillTypeGenerator.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEVELOP
            IEnumerable<JoinProxyModel1> jplist = new List<JoinProxyModel1>();

            jplist.SelectTestGet();
#else
            List<JoiningModel1> jList = new List<JoiningModel1>();

            var selectTest = jList.SelectGet2();

            JoiningModel1 n = new JoiningModel1();

            var convertTest = n.ToGet2();

            List<ProxyModel1> pItems = new List<ProxyModel1>();

            pItems.SelectTestGet();

            List<WithModelName1> aa = new List<WithModelName1>();

            var t = aa.Selectabc2().FirstOrDefault();
            var t2 = aa.Selectabc1();


            List<WithModelName4> bb = new List<WithModelName4>();

            bb.Selectmodel1().FirstOrDefault();
            bb.Selectmodel2().FirstOrDefault();
            bb.Selectmodel3().FirstOrDefault();

            Test1Model tmodel1 = new Test1Model() { TestValue1 = "abcxaca", TestValue2 = "35t2452" };

            Test2Model tmodel2 = new Test2Model() { };

            tmodel1.FillTo(tmodel2);
#endif
        }
    }
}