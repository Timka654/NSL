﻿using NSL.Generators.FillTypeGenerator.Attributes;

namespace NSL.Generators.FillTypeGenerator.Tests
{
    public partial class Test5Model
    {
        public int Abc4 { get; set; }
    }


    public partial class Test6Model
    {
        public int Abc1 { get; set; }
        public int Abc2 { get; set; }
        public int Abc3 { get; set; }
        public int Abc4 { get; set; }
    }

    //[FillTypeGenerate(typeof(Test6Model), null)]
    //[FillTypeGenerate(typeof(Test6Model))]
    //[FillTypeGenerate(typeof(Test6Model), null, "a1")]
    [FillTypeGenerate(typeof(Test6Model), "a1")]
    [FillTypeGenerate(typeof(Test6Model), "a1")]
    [FillTypeGenerate(typeof(Test6Model),"a2")]
    [FillTypeGenerate(typeof(Test6Model),"a3")]
    [FillTypeGenerate(typeof(Test6Model),"a4")]
    [FillTypeGenerate(typeof(Test5Model),"a4")]
    public partial class Test7Model
    {
        [FillTypeGenerateInclude("a1")]
        public int Abc1 { get; set; }
        [FillTypeGenerateInclude("a2")]
        public int Abc2 { get; set; }
        [FillTypeGenerateInclude("a3")]
        public int Abc3 { get; set; }
        [FillTypeGenerateInclude("a4")]
        public int Abc4 { get; set; }
    }
}
