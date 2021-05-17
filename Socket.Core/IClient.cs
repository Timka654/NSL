﻿using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Net;

namespace SocketCore
{
    public delegate void ReceivePacketDebugInfo<T>(T client, ushort pid, int len) where T : IClient;
#if DEBUG
    public delegate void SendPacketDebugInfo<T>(T client, ushort pid, int len, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0) where T : IClient;
#else
    public delegate void SendPacketDebugInfo<T>(T client, ushort pid, int len) where T : IClient;
#endif
    public interface IClient
    {
        void Send(OutputPacketBuffer packet
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            );

        void SendEmpty(ushort packetId
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            );

        void Send(ushort packetId, byte value
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            );

        void Send(ushort packetId, int value
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            );

        void Send(ushort packetId, bool value
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            );

        void Send(ushort packetId, short value
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            );

        void Send(ushort packetId, ushort value
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            );

        void Send(ushort packetId, uint value
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            );

        void Send(ushort packetId, long value
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            );

        void Send(ushort packetId, ulong value
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            );

        void Send(ushort packetId, float value
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            );

        void Send(ushort packetId, double value
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            );

        void Send(ushort packetId, DateTime value
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Max len 64 536</param>
        /// <param name="memberName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        void Send(ushort packetId, string value
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            );

//        void SendSerialize<O>(ushort packetId, O obj, string scheme
//#if DEBUG
//            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
//            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
//            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
//#endif
//            );

        void Send(byte[] buf, int offset, int lenght);

        void Disconnect();

        bool GetState();

        IPEndPoint GetRemovePoint();

        void ChangeUserData(INetworkClient data);

        object GetUserData();

        System.Net.Sockets.Socket GetSocket();
    }
}
