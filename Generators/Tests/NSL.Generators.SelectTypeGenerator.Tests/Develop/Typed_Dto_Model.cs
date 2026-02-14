using NSL.Generators.SelectTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.SelectTypeGenerator.Tests.Develop
{
    [SelectGenerate("DtoTest1", Dto = true, Typed = true)]
    public class Typed_Dto1_Model
    {
        [SelectGenerateInclude("DtoTest1")]
        [SelectGenerateProxy("TestModel")]
        public Typed_Dto3_Model Item { get; set; }
    }

    [SelectGenerate("DtoTest2", Dto = true, Typed = true)]
    public class Typed_Dto2_Model
    {
        [SelectGenerateInclude("DtoTest2")]
        [SelectGenerateProxy("TestModel")]
        public Typed_Dto3_Model Item { get; set; }
    }
    public class Typed_Dto3_Model
    {

        [SelectGenerateInclude("TestModel")]
        public int Id { get; set; }
    }
}
