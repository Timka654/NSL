#if !DEVELOP

using NSL.Generators.FillTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop
{
    [FillTypeFromGenerate(typeof(StringCast2Model))]
    partial class StringCast1Model
    {
        //[FillTypeGenerateConvert(typeof(StringBaseConvert2))]
        [FillTypeGenerateConvert(typeof(StringBaseConvert))]
        public string Property1 { get; set; }
    }

    partial class StringCast2Model
    {
        [FillTypeGenerateConvert(typeof(StringBaseConvert))]
        public IStringCastInterface Property1 { get; set; }
    }

    public partial interface IStringCastInterface
    {

    }

    public class StringValueCast : IStringCastInterface
    {
        public string Value { get; set; }
    }

    public class StringBaseConvert
    {
        public static string Convert(IStringCastInterface value)
        {
            if (value is StringValueCast svc) return svc.Value;

            return null;
        }

        public static IStringCastInterface Convert(string value)
        {
            return new StringValueCast()
            {
                Value = value
            };
        }
    }

    //public class StringBaseConvert2
    //{
    //    public static string Convert(IStringCastInterface value)
    //    {
    //        if (value is StringValueCast svc) return svc.Value;

    //        return null;
    //    }

    //    public static IStringCastInterface Convert(string value)
    //    {
    //        return new StringValueCast()
    //        {
    //            Value = value
    //        };
    //    }
    //}
}

#endif