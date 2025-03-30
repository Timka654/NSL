using NSL.Generators.SelectTypeGenerator.Attributes;

namespace NSL.Generators.SelectTypeGenerator.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEVELOP
            IEnumerable<JoinProxyModel1> jplist = new List<JoinProxyModel1>();
            DTOModel1Dtotemp1Model dto = new DTOModel1Dtotemp1Model();
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

    //[SelectGenerate("temp1")]
    //public partial class CollectionSelectorModel1
    //{
    //    [SelectGenerateInclude("temp1")]
    //    public CollectionSelectorModel1[] items { get; set; }

    //    [SelectGenerateInclude("temp1")]
    //    public CollectionSelectorModel1 item { get; set; }
    //}

    [SelectGenerate("temp1", Typed = true)]
    public partial class ReadOnlyTyped1
    {
        [SelectGenerateInclude("temp1")]
        public int MyProperty1 { get; }

        [SelectGenerateInclude("temp1")]
        public int MyProperty2 { set { } }
    }

    public partial class DTOInnerTypeModel
    {
        [SelectGenerateInclude("temp1")]
        public int MyProperty1 { get; set; }

        [SelectGenerateInclude("temp1")]
        public int MyProperty2 { get; set; }
    }


    [SelectGenerate("temp1", Typed = true, Dto = true)]
    public partial class DTOTypedModel1
    {
        [SelectGenerateInclude("temp1")]
        [SelectGenerateProxy("temp1")]
        public ReadOnlyTyped1 Type { get; set; }

        [SelectGenerateInclude("temp1")]
        [SelectGenerateProxy("temp1")]
        public ReadOnlyTyped1[] ArrayType { get; set; }

        [SelectGenerateInclude("temp1")]
        public DTOInnerTypeModel Type2 { get; set; }

        [SelectGenerateInclude("temp1")]
        public DTOInnerTypeModel[] ArrayType2 { get; set; }

        [SelectGenerateInclude("temp1")]
        public int BaseType { get; set; }

        [SelectGenerateInclude("temp1")]
        public int? BaseNulledType { get; set; }

        [SelectGenerateInclude("temp1")]
        public string? StringType { get; set; }
    }


    [SelectGenerate("temp1", Dto = true)]
    public partial class DTOModel1
    {
        public ReadOnlyTyped1 Type { get; set; }

        public ReadOnlyTyped1[] ArrayType { get; set; }

        [SelectGenerateInclude("temp1")]
        public int BaseType { get; set; }

        [SelectGenerateInclude("temp1")]
        public int? BaseNulledType { get; set; }

        [SelectGenerateInclude("temp1")]
        public string? StringType { get; set; }
    }
}