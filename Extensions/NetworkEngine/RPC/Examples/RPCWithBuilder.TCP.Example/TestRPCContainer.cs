using NSL.Extensions.RPC;
using NSL.Extensions.RPC.Generator;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPCWithBuilder.TCP.Example
{

    internal partial class TestRPCClientContainer : RPCHandleContainer<NetworkClient>
    {
        [RPCMethod]
        public void abc1(int v1, int? v2, string v3, string? v4, testData v5, testStruct v6)
        {

        }

        //[RPCMethod]
        //public void abc1(testData data, testData data2, testStruct str, int val, testStruct? emptyStr)
        //{

        //}

        //[RPCMethod]
        //public void abc2(testData data)
        //{

        //}
    }
}
