using NSL.Generators.SelectTypeGenerator.Attributes;

namespace NSL.Generators.SelectTypeGenerator.Tests
{
    [SelectGenerateModelJoin("TestGet", "BaseGet")]
    public partial class JoinProxyModel1 { 
    
    }

    [SelectGenerate("BaseGet")]
    [SelectGenerate("TestGet")]
    public partial class JoinProxyModel1
    {

        [SelectGenerateInclude("BaseGet")]
        public int Id { get; set; }

        [SelectGenerateInclude("TestGet")]
        [SelectGenerateProxy("TestGet", "Get")]
        public JoinProxyModel2 J1 { get; set; }


        //    [SelectGenerateInclude("TestGet")]
        //    [SelectGenerateProxy("Get")]
        //    public virtual List<JoinProxyModel2> JP1 { get; set; }

        //    [SelectGenerateInclude("TestGet")]
        //    [SelectGenerateProxy("Get")]
        //    public virtual JoinProxyModel2 JP2 { get; set; }

        //    [SelectGenerateInclude("TestGet")]
        //    [SelectGenerateProxy("Get_n")]
        //    public virtual List<JoinProxyModel2> JP3 { get; set; }

        //    [SelectGenerateInclude("TestGet")]
        //    [SelectGenerateProxy("Get_n")]
        //    public virtual JoinProxyModel2 JP4 { get; set; }

        //    [SelectGenerateInclude("TestGet")]
        //    [SelectGenerateProxy("GetA")]
        //    public virtual List<JoinProxyModel2> JP5 { get; set; }

        //    [SelectGenerateInclude("TestGet")]
        //    [SelectGenerateProxy("GetA")]
        //    public virtual JoinProxyModel2 JP6 { get; set; }
        }
    }