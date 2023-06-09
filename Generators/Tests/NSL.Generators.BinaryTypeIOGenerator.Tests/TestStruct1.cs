using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.BinaryTypeIOGenerator.Tests
{
    [BinaryIOType]
    public partial class TestStruct1
    {
        public string s1 { get; set; }

        public int n1 { get; set; }

        [BinaryIOWriteMethod(For = "abc")]
        public partial void BinaryWriteFull(OutputPacketBuffer packet);

        [BinaryIOReadMethod]
        public static partial TestStruct1 BinaryReadFull(InputPacketBuffer data);
    }
}
