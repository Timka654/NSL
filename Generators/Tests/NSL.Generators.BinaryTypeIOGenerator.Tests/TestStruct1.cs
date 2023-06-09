﻿using NSL.Generators.BinaryTypeIOGenerator.Attributes;
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
        [BinaryIOData(For = "abc")]
        public string s1 { get; set; }

        public int n1 { get; set; }

        [BinaryIOWriteMethod]
        public partial void BinaryWriteFull(OutputPacketBuffer packet);

        [BinaryIOReadMethod]
        public static partial TestStruct1 BinaryReadFull(InputPacketBuffer data);

        [BinaryIOWriteMethod(For = "abc")]
        public partial void BinaryWriteABC(OutputPacketBuffer packet);

        [BinaryIOReadMethod(For = "abc")]
        public static partial TestStruct1 BinaryReadABC(InputPacketBuffer data);
    }
}
