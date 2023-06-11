using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.BinaryTypeIOGenerator.Tests
{
    public class TestStruct2
    {
        [BinaryIOData(For = "en")] public int ab1 { get; set; }
    }
}
