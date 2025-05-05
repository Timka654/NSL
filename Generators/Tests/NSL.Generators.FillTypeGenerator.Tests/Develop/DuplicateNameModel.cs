using NSL.Generators.FillTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop
{
    public interface IDuplicateNameDataModel<TData>
    {
        public TData Data { get; set; }
    }

    [FillTypeFromGenerate(typeof(IDuplicateNameModel))]
    public partial interface IDuplicateNameModel
    {
        string Id { get; set; }
    }

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

    internal partial class DuplicateNameToModel : IDuplicateNameModel
    {
        public string Id { get; set; }
    }

    [FillTypeGenerate(typeof(StaticModel))]
    partial class StaticModel
    {
        public static object a { get; set; } = new object();

        public int b { get; set; }  
    }

}
