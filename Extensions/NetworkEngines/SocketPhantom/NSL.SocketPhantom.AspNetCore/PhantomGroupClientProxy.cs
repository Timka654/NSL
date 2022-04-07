﻿using Microsoft.AspNetCore.SignalR;
using SocketPhantom.AspNetCore.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketPhantom.AspNetCore
{
    public class PhantomGroupClientProxy : IClientProxy
    {
        public ConcurrentDictionary<string, PhantomHubClientProxy> Clients = new();

        public async Task SendCoreAsync(string method, object[] args, CancellationToken cancellationToken = default)
        {
            await Task.WhenAll(Clients.Values.Select(x => x.SendCoreAsync(method, args, cancellationToken)));
        }
    }
}
