using NSL.SocketCore.Utils;
using NSL.SocketServer.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace NSL.UDP.Client
{
    public class UDPClient<TClient> : BaseUDPClient<TClient, UDPClient<TClient>>
        where TClient : BaseNetworkConnection, new()
    {
        private TClient clientData;

        public override TClient Data => clientData;

        private bool connectDeferred;

        /// <summary>
        /// The session GUID most recently acknowledged via UDPConnectHandshake.
        /// When a new handshake arrives with a different GUID (meaning the client started a fresh
        /// connect attempt) channels and ciphers are rebuilt.  Repeated probes from the same
        /// connect attempt carry the same GUID and are ignored.
        /// Reset to <see cref="Guid.Empty"/> by <see cref="Reinitialize"/> so the very first
        /// handshake after a reconnect always triggers a rebuild.
        /// </summary>
        internal Guid LastHandshakeSessionId;

        public UDPClient(IPEndPoint receivePoint, Socket listenerSocket, UDPClientOptions<TClient> options, bool deferConnect = false) : base(receivePoint, listenerSocket, options)
        {
            connectDeferred = deferConnect;
            Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();

            PacketHandles = options.GetHandleMap();

            clientData = new TClient();

            //обзятельная переменная в NetworkClient, для отправки данных, можно использовать привидения типов (Client)NetworkClient но это никому не поможет
            Data.Network = this;
            //Data.Options = options;

            //установка криптографии для дешифровки входящих данных, указана в общих настройках сервера
            inputCipher = options.InputCipher.CreateEntry();
            //установка криптографии для шифровки исходящих данных, указана в общих настройках сервера
            outputCipher = options.OutputCipher.CreateEntry();

            disconnected = false;

            //RunException(new Exception($"[UDP-DIAG] Initialize ep={GetRemotePoint()} deferred={connectDeferred}"));

            if (!connectDeferred)
                options.CallClientConnectEvent(Data);
        }

        public override void SetClientData(BaseNetworkConnection from)
        {
            if (from == null)
            {
                clientData = null;
                return;
            }

            if (from is TClient td)
            {
                // current data for dispose and move data
                var oldData = clientData;

                clientData = td;
                clientData.Network = this;

                oldData.Network = null;

                from.ChangeOwner(oldData);

                return;
            }

            throw new Exception($"{nameof(from)} must have type {typeof(TClient)}");
        }

        protected override UDPClient<TClient> GetParent() => this;

        protected override void OnReceive(ushort pid, int len)
        {
            Data.LastReceiveMessage = DateTime.UtcNow;

            if (connectDeferred)
            {
                connectDeferred = false;
                options.CallClientConnectEvent(Data);
            }

            base.OnReceive(pid, len);
        }

        protected override void RunDisconnect() => base.options.CallClientDisconnectEvent(Data);

        protected override void RunException(Exception ex) => base.options.CallExceptionEvent(ex, Data);

        /// <summary>
        /// Resets this <see cref="UDPClient{TClient}"/> to a freshly-connected state so it can be
        /// reused when the remote endpoint reconnects without reallocating the dictionary entry on
        /// the server side.  Safe to call concurrently — the first caller wins via lock(this).
        /// </summary>
        internal void Reinitialize()
        {
            //RunException(new Exception($"[UDP-DIAG] Reinitialize called ep={GetRemotePoint()}"));

            lock (this)
            {
                if (!IsDisconnected)
                {
                    //RunException(new Exception($"[UDP-DIAG] Reinitialize skipped (not disconnected) ep={GetRemotePoint()}"));
                    return;
                }

                if (clientData != null)
                    clientData.Network = null;

                clientData = new TClient();
                clientData.Network = this;

                PacketHandles = options.GetHandleMap();
                connectDeferred = false;

                // Ciphers are disposed in Disconnect() — recreate them immediately so the very
                // first arriving packet can be decrypted.  Cipher instances carry no sequence
                // state, so this is safe and cannot cause desync.
                // Channels are intentionally NOT reset here; they are reset only when a genuine
                // UDPConnectHandshake is received (via ReinitializeChannels()), to avoid
                // flushing the reliable-channel sequence numbers on spurious reconnect paths.
                if (inputCipher != null) inputCipher.Dispose();
                if (outputCipher != null) outputCipher.Dispose();
                inputCipher = options.InputCipher.CreateEntry();
                outputCipher = options.OutputCipher.CreateEntry();

                ReinitializeBase();
            }

            // Reset session ID so the first handshake from the new connect attempt is always
            // treated as a new session, regardless of what GUID the client sends.
            LastHandshakeSessionId = Guid.Empty;

            //RunException(new Exception($"[UDP-DIAG] Reinitialize done ep={GetRemotePoint()}"));

            // Fire connect event outside the lock.
            options.CallClientConnectEvent(clientData);
        }
    }
}