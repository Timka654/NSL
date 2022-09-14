﻿using NSL.Extensions.RPC;
using NSL.Extensions.RPC.Generator;
using NSL.Extensions.RPC.Generator.Attributes;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using RPCWithBuilder.TCP.Example.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPCWithBuilder.TCP.Example
{

    internal partial class TestRPCClientContainer<TClient> : RPCHandleContainer<TClient>
        where TClient : INetworkClient
    {
        [RPCMethod]
        public virtual TestStructModel abc1(int v1, int? v2, string v3, string? v4, TestDataModel v5, TestStructModel v6)
        {
            int random = Random.Shared.Next();


            if (IsServer())
                Console.WriteLine($"[Server] {nameof(abc1)} called... {nameof(random)} have {random} value");
            else
                Console.WriteLine($"[Client] {nameof(abc1)} called... {nameof(random)} have {random} value");

            abc2(random, v2, v3, v4, v5, v6);

            return new TestStructModel() { tsValue = random };
        }

        [RPCMethod]
        public virtual void abc2(int v1, int? v2, string v3, string? v4, TestDataModel v5, TestStructModel v6)
        {

            if (IsServer())
                Console.WriteLine($"[Server] {nameof(abc2)} called... with {nameof(v1)} have value {v1}");
            else
                Console.WriteLine($"[Client] {nameof(abc2)} called... with {nameof(v1)} have value {v1}");
        }

        //[RPCMethod]
        //public void abc1(testData data, testData data2, testStruct str, int val, testStruct? emptyStr)
        //{

        //}

        //[RPCMethod]
        //public void abc2(testData data)
        //{

        //}

        private bool IsServer()
        {
            return NetworkClient is NetworkServerClient;
        }

        private bool IsClient()
        {
            return NetworkClient is NetworkClient;
        }
    }
}
