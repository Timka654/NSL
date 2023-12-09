﻿using NSL.Generators.SelectTypeGenerator.Attributes;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop.WithModelName
{
    public partial class WithModelName5
    {
        [SelectGenerateInclude("model4", "model6")]
        public int Abc1 { get; set; }

        [SelectGenerateInclude("model5", "model6")]
        public int Abc2 { get; set; }
    }
}
