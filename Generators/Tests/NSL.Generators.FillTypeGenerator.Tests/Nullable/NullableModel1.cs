#if !DEVELOP
using NSL.Generators.FillTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop.Nullable
{
    internal partial class NullableModel1
    {
        public int? a { get; set; }

        public int b { get; set; }

        public NullableData1Model Data1 { get; set; }

        public NullableData1Model? Data2 { get; set; }

        public NullableData1Model[] Data3 { get; set; }

        public NullableData1Model[]? Data4 { get; set; }
    }

    [FillTypeGenerate(typeof(NullableModel1))]
    internal partial class NullableModel2
    {
        public int? a { get; set; }

        public int b { get; set; }

        public NullableData1Model Data1 { get; set; }

        public NullableData1Model? Data2 { get; set; }

        public NullableData1Model[] Data3 { get; set; }

        public NullableData1Model[]? Data4 { get; set; }
    }

    [FillTypeGenerate(typeof(NullableModel1))]
    internal partial class NullableModel3
    {
        public int a { get; set; }

        public int? b { get; set; }

        public NullableData2Model Data1 { get; set; }

        public NullableData2Model? Data2 { get; set; }

        public NullableData2Model[] Data3 { get; set; }

        public NullableData2Model[]? Data4 { get; set; }
    }


    public partial class NullableData1Model
    {
        public int? a { get; set; }

        public int b { get; set; }
    }

    public partial class NullableData2Model
    {
        public int a { get; set; }

        public int? b { get; set; }
    }
}
#endif