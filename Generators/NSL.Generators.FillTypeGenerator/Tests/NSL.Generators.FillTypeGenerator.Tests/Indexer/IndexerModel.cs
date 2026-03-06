#if !DEVELOP
using NSL.Generators.FillTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop
{
    [FillTypeGenerate(typeof(IndexerModel))]
    internal partial class IndexerModel
    {
        public object this[int index]
        {
            get => default;
            set
            {
            }
        }

        public bool property1 { get; set; }

        public bool field1 { get; set; }
    }
}
#endif