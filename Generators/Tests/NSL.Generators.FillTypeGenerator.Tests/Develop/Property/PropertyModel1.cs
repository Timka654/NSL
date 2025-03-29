using NSL.Generators.FillTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop.Property
{
    [FillTypeGenerate(typeof(PropertyModel2))]
    [FillTypeGenerate(typeof(PropertyModel3))]
    [FillTypeFromGenerate(typeof(PropertyModel2))]
    [FillTypeFromGenerate(typeof(PropertyModel3))]
    [FillTypeFromGenerate(typeof(PropertyModel4))]
    public partial class PropertyModel1
    {
        public int a1 { get; set; }

        public int[] a2 { get; set; }
        public int? a3 { get; set; }
        public int[]? a4 { get; set; }
    }

    public partial class PropertyModel2
    {
        public int a1 { get; }
        public int[] a2 { get; }
        public int? a3 { get; }
        public int[]? a4 { get; }
    }

    public partial class PropertyModel4
    {
        public int a1 => 1;
        public int[] a2 => Enumerable.Repeat(1,1).ToArray();
        public int? a3 => 1;
        public int[]? a4 => Enumerable.Repeat(1, 1).ToArray();
    }

    public partial class PropertyModel3
    {
        public int a1 { set { } }
        public int[] a2 { set { } }
        public int? a3 { set { } }
        public int[]? a4 { set { } }
    }
}
