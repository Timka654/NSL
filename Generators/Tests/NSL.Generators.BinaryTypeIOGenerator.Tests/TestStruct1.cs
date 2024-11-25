using NSL.Generators.BinaryTypeIOGenerator.Attributes;
//using ZergRush.ReactiveCore;

namespace NSL.Generators.BinaryTypeIOGenerator.Tests.abserb.erge.ergeg.erg
{
    public enum abcEn
    {
        abc1,
        abc2
    }
}



namespace NSL.Generators.BinaryTypeIOGenerator.Tests
{

    public struct abcfff{
        public int abc1;
    }


    //[NSLBIOType("en", "abc")]
    //[NSLBIOType]
    public partial class TestStruct1 :TestStruct2
    {
        [NSLBIOInclude("en")]
        public TestStruct1 testArr { get; set; }
        //[NSLBIOInclude(For = "en")]
        //public ReactiveCollection<byte> data { get; set; }

        [NSLBIOInclude("en")]
        public NSL.Generators.BinaryTypeIOGenerator.Tests.abserb.erge.ergeg.erg.abcEn en1 { get; set; }

        [NSLBIOInclude("en")]
        public TestStruct2 en2 { get; set; }

        [NSLBIOInclude("en")]
        public NSL.Generators.BinaryTypeIOGenerator.Tests.abserb.erge.ergeg.erg.abcEn? en3 { get; set; }

        //[NSLBIOInclude("en")]
        //public abcfff en4 { get; set; }

        //[NSLBIOInclude("en")]
        //public abcfff? en5 { get; set; }

        [NSLBIOInclude("abc")]
        public string s1 { get; set; }

        [NSLBIOInclude]
        public int n1 { get; set; }

        //[BinaryIOWriteMethod]
        //public partial void BinaryWriteFull(OutputPacketBuffer packet);

        //[BinaryIOReadMethod]
        //public static partial TestStruct1 BinaryReadFull(InputPacketBuffer data);

        //[BinaryIOWriteMethod(For = "abc")]
        //public partial void BinaryWriteABC(OutputPacketBuffer packet);

        //[BinaryIOReadMethod(For = "abc")]
        //public static partial TestStruct1 BinaryReadABC(InputPacketBuffer data);

        //[BinaryIOReadMethod(For = "en")]
        //public static partial TestStruct1 BinaryReaden(InputPacketBuffer data);

        //[BinaryIOWriteMethod(For = "en")]
        //public partial void BinaryWriteen(OutputPacketBuffer data);
    }
}
