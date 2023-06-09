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

        [CLSCompliant(true)]
        [BinaryIOWriteMethod(For = "abc")]
        static partial void BinaryWriteFull(TestStruct1 data, OutputPacketBuffer packet);

        [CLSCompliant(true)]
        [BinaryIOReadMethod]
        static partial void BinaryReadFull(InputPacketBuffer data);
    }
}
