#if !DEVELOP
using NSL.Generators.FillTypeGenerator.Attributes;
using NSL.Generators.FillTypeGenerator.Tests.DuplicateName;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop
{
    [FillTypeGenerate(typeof(DuplicateNameToModel))]
    internal partial class DuplicateNameModel : IDuplicateNameModel
    {
        public string Id { get; set; }
    }

    [FillTypeGenerate(typeof(DuplicateNameToModel))]
    internal partial class DuplicateNameModel<TData> : IDuplicateNameDataModel<TData>, IDuplicateNameModel
    {
        public string Id { get; set; }

        public TData Data { get; set; }
    }

}
#endif
