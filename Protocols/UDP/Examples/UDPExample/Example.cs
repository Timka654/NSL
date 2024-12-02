using NSL.SocketCore.Utils.Cipher;
using NSL.SocketCore.Utils.Logger;
using NSL.UDP;
using NSL.UDP.Info;
using NSL.UDP.Interface;
using NSL.UDP.Packet;
using System;

namespace UDPExample
{
    public class Example<TOptions>
        where TOptions : UDPClientOptions<NetworkClient>, IBindingUDPOptions, new()
    {
        protected TOptions options;

        protected void Initialize(IBasicLogger logger)
        {
            options = new TOptions();

            options.BindingIP = "0.0.0.0";

            options.ReceiveBufferSize = 1024;

            options.HelperLogger = logger;

            options.StunServers.Add(new StunServerInfo("stun.l.google.com:19302"));
            options.StunServers.Add(new StunServerInfo("stun1.l.google.com:19302"));
            options.StunServers.Add(new StunServerInfo("stun2.l.google.com:19302"));
            options.StunServers.Add(new StunServerInfo("stun3.l.google.com:19302"));
            options.StunServers.Add(new StunServerInfo("stun4.l.google.com:19302"));
            options.StunServers.Add(new StunServerInfo("stun.node4.co.uk"));
            options.StunServers.Add(new StunServerInfo("stun.nventure.com"));
            options.StunServers.Add(new StunServerInfo("stun.patlive.com"));
            options.StunServers.Add(new StunServerInfo("stun.petcube.com"));
            options.StunServers.Add(new StunServerInfo("stun.phoneserve.com"));
            options.StunServers.Add(new StunServerInfo("stun.prizee.com"));
            options.StunServers.Add(new StunServerInfo("stun.qvod.com"));
            options.StunServers.Add(new StunServerInfo("stun.refint.net"));
            options.StunServers.Add(new StunServerInfo("stun.remote-learner.net"));
            options.StunServers.Add(new StunServerInfo("stun.rounds.com"));
            options.StunServers.Add(new StunServerInfo("stun.samsungsmartcam.com"));
            options.StunServers.Add(new StunServerInfo("stun.sysadminman.net"));
            options.StunServers.Add(new StunServerInfo("stun.tatneft.ru"));
            options.StunServers.Add(new StunServerInfo("stun.telefacil.com"));
            options.StunServers.Add(new StunServerInfo("stun.ucallweconn.net"));
            options.StunServers.Add(new StunServerInfo("stun.virtual-call.com"));
            options.StunServers.Add(new StunServerInfo("stun.voxgratia.org"));

            options.RegisterUDPPingHandle();

            options.OnClientConnectEvent += Options_OnClientConnectEvent;
            options.OnClientDisconnectEvent += Options_OnClientDisconnectEvent;

            options.OnExceptionEvent += Options_OnExceptionEvent;

            options.InputCipher = new PacketNoneCipher();

            options.OutputCipher = new PacketNoneCipher();
        }

        private void Options_OnExceptionEvent(Exception ex, NetworkClient client)
        {
            options.HelperLogger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Error, ex.ToString());
        }

        private void Options_OnClientConnectEvent(NetworkClient client)
        {
            options.HelperLogger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"Client Connected");
        }

        private void Options_OnClientDisconnectEvent(NetworkClient client)
        {
            options.HelperLogger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"Client disconnected");
        }
    }
}
