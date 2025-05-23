﻿#if !DEVELOP
using NSL.Generators.SelectTypeGenerator.Attributes;

namespace NSL.Generators.SelectTypeGenerator.Tests
{
    [SelectGenerate("TestGet")]
    public partial class JoinProxyModel1
    {
        [SelectGenerateInclude("TestGet")]
        [SelectGenerateProxy("Get")]
        public virtual List<JoinProxyModel2> JP1 { get; set; }

        [SelectGenerateInclude("TestGet")]
        [SelectGenerateProxy("Get")]
        public virtual JoinProxyModel2 JP2 { get; set; }

        [SelectGenerateInclude("TestGet")]
        [SelectGenerateProxy("Get_n")]
        public virtual List<JoinProxyModel2> JP3 { get; set; }

        [SelectGenerateInclude("TestGet")]
        [SelectGenerateProxy("Get_n")]
        public virtual JoinProxyModel2 JP4 { get; set; }

        [SelectGenerateInclude("TestGet")]
        [SelectGenerateProxy("GetA")]
        public virtual List<JoinProxyModel2> JP5 { get; set; }

        [SelectGenerateInclude("TestGet")]
        [SelectGenerateProxy("GetA")]
        public virtual JoinProxyModel2 JP6 { get; set; }
    }
}
#endif