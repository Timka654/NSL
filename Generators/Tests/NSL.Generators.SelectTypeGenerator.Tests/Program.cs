using NSL.Generators.SelectTypeGenerator.Attributes;

namespace NSL.Generators.SelectTypeGenerator.Tests
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

            var t = aa.SelectTypedabc2().FirstOrDefault();
            var t2 = aa.SelectTypedabc1();


            List<WithModelName4> bb = new List<WithModelName4>();

            bb.Selectmodel1().FirstOrDefault();
            bb.Selectmodel2().FirstOrDefault();
            bb.Selectmodel3().FirstOrDefault();

#endif
        }
    }

    [SelectGenerate("temp1")]
    public partial class CollectionSelectorModel1
    {
        [SelectGenerateInclude("temp1")]
        public CollectionSelectorModel1[] items { get; set; }

        [SelectGenerateInclude("temp1")]
        public CollectionSelectorModel1 item { get; set; }
    }

    [SelectGenerate("temp1", Typed = true)]
    public partial class ReadOnlyTyped1
    {
        [SelectGenerateInclude("temp1")]
        public int MyProperty1 { get; }

        [SelectGenerateInclude("temp1")]
        public int MyProperty2 { set { } }
    }
}