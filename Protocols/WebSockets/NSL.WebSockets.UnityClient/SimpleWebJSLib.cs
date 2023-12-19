using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NSL.WebSockets.UnityClient
{
    internal static class SimpleWebJSLib
    {
        [DllImport("__Internal")]
        internal static extern bool IsConnected(int index);

#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
        [DllImport("__Internal")]
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
        internal static extern void Connect(string address, Action<int> openCallback, Action<int> closeCallBack, Action<int, IntPtr, int> messageCallback, Action<int> errorCallback, int index);

        [DllImport("__Internal")]
        internal static extern void Disconnect(int index);

        [DllImport("__Internal")]
        internal static extern bool Send(int index, byte[] array, int offset, int length);
    }
}
