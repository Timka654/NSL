﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCL;
using SocketCore.Utils.Buffer;

namespace SCL.Utils.SystemPackets
{
    public class SystemTime<T> : IPacket<T> where T:BaseSocketNetworkClient
    {
        public SystemTime(ClientOptions<T> options) : base(options)
        {
        }

        protected override void Receive(InputPacketBuffer data)
        {
            var now = DateTime.Now;

            var dt = data.ReadDateTime().Value;
            try
            {
                Client.ServerDateTimeOffset = now - dt;

            }
            catch
            {
                //ThreadHelper.InvokeOnMain(() => { UnityEngine.Debug.Log($"{now} - {dt} =  {now - dt}"); });
            }
        }
        
    }
}