using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.Generators.BinaryTypeIOGenerator.Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var o = new OutputPacketBuffer();

            var ouData = new TestStruct1() { s1 = " abbbbcc", n1 = 75656/*, data = new ZergRush.ReactiveCore.ReactiveCollection<byte>()*/ };

            ouData.WriteabcTo(o);

            var i = new InputPacketBuffer(o.CompilePacket());

            var s = TestStruct1.ReadabcFrom(i);





            var o1 = new OutputPacketBuffer();

            ouData = new TestStruct1() { s1 = "rwegrewgwrg", n1 = 6347/*, data = new ZergRush.ReactiveCore.ReactiveCollection<byte>(new byte[] { 45, 67, 85 })*/ };
            ouData.WriteenTo(o1);

            var i1 = new InputPacketBuffer(o1.CompilePacket());

            var s1 = TestStruct1.ReadenFrom(i1);

        }
    }

    //[NSLBIOType("a1", "a2", "a3", null)]
    //[NSLBIOModelJoin("a3", "a2", "a1")]
    //public partial class a1
    //{
    //    [NSLBIOInclude("a1")]
    //    public string t1 { get; set; }


    //    [NSLBIOInclude("a2")]
    //    public string t2 { get; set; }


    //    [NSLBIOInclude("a3")]
    //    public string t3 { get; set; }


    //    [NSLBIOInclude]
    //    public string t4 { get; set; }


    //    [NSLBIOInclude(null, "a3")]
    //    public string t5 { get; set; }
    //}
    //[NSLBIOType("a1", "a2", null)]
    //public partial class a2
    //{
    //    [NSLBIOInclude("a1", "a2")]
    //    [NSLBIOProxy("a2", "a2")]
    //    [NSLBIOProxy(null)]
    //    public b2 b2p { get; set; }

    //    [NSLBIOInclude]
    //    public int b1 { get; set; }
    //}

    //[NSLBIOModelJoin("a2","a3")]
    //public partial class b2
    //{
    //    [NSLBIOInclude]
    //    public string t1 { get; set; }

    //    [NSLBIOInclude("a2", null)]
    //    public string t2 { get; set; }

    //    [NSLBIOInclude("a3")]
    //    public string t3 { get; set; }
    //}
    //[NSLBIOType]
    //public partial class AuctionBidResponseModel
    //{
    //    public AuctionNewBidResultEnum Result { get; set; }

    //    public double NewBidValue { get; set; }

    //    public AuctionBidResponseModel d { get; set; }
    //}
    //public enum AuctionNewBidResultEnum : byte
    //{
    //    Success,
    //    NoFound,
    //    NoBidRange,
    //    NoMoney,
    //    OwnedItem
    //}


    //[NSLBIOType("Response", "Sync", null)]
    //[NSLBIOModelJoin("Sync", "Response")]
    //public partial class a1Class
    //{
    //    [NSLBIOInclude("Response"), NSLBIOProxy("aaa")] public a2Class a2 { get; set; }
    //}

    //[NSLBIOModelJoin("bbb", "aaa")]
    //public partial class a2Class
    //{
    //    [NSLBIOInclude("aaa")] public int a { get; set; }
    //}

    [NSLBIOType("Response")]
    public partial class a1Class
    {
        [NSLBIOInclude("Response")] public a2Class a2 { get; set; }
    }

    public partial class a2Class
    {
        [NSLBIOInclude("Response")] public int a { get; set; }
    }
}