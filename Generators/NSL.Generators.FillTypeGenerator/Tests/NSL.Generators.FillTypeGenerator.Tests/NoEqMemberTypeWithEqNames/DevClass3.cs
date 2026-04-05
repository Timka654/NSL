#if !DEVELOP

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.NoEqMemberTypeWithEqNames
{
    public partial class DevClass3
    {
        public DevEnum Evalue { get; set; }

        public int dValue { get; set; }

        public pt pc1Test { get; set; }
        public pt pc2Test { get; set; }
    }


    public class pt { }

    public class ct1
    {
        public static implicit operator pt(ct1 c)
        {
            return new pt();
        }
    }

    public class ct2 : pt
    {
    }
}

#endif