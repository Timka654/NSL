﻿using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProxyHostClient.Packets.Player
{
    [ProxyHostPacket(Enums.ClientPacketsEnum.PlayerDisconnected)]
    internal class PlayerDisconnected : IPacket<ProxyHostClientData>
    {
        public void Receive(ProxyHostClientData client, InputPacketBuffer data)
        {
        }

        public static void Send(ProxyHostClientData client, Guid id)
        {

        }
    }
}
