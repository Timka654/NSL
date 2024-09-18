using NSL.Generators.FillTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop.Cast
{
    internal class CastContainer1
    {
    }

    [FillTypeGenerate(typeof(CastContainer2))]
    internal partial class CastContainer2
    {

    }


    public class castT1 : castT2
    {
    }
    public class castT2
    {
        public int i { get; set; }
    }
    public class castT3 : castT2
    {
    }
}
