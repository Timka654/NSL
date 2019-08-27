using Mono.Nat;
using SCL.Node.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SCL.Node
{
    public class INetworkNode<T> : MonoBehaviour
        where T : INodePlayer
    {
        public delegate void CommandHandle(T player, NodeInputPacketBuffer buffer);

#if DEBUG
        public event CommandHandle OnReceivePacket;

        protected void AppendOnReceiveData(T player, NodeInputPacketBuffer packet)
        {
            _actions.Enqueue(() => { OnReceivePacket?.Invoke(player, packet); });
        }
#endif

        protected Socket _socket;

        protected ManualResetEvent _uPnPLocker = new ManualResetEvent(false);

        protected bool InitResult { get; set; } = false;

        public int Port { get; protected set; }

        protected Protocol Protocol { get; set; }

        protected readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

        protected readonly ConcurrentDictionary<int, T> _players = new ConcurrentDictionary<int, T>();

        protected readonly Dictionary<byte, CommandHandle> _commands = new Dictionary<byte, CommandHandle>();

        public int MyPlayerId { get; set; }

        public virtual bool Initiliaze(string ip, ref int port, int myPlayerId)
        {
            MyPlayerId = myPlayerId;

            NatUtility.DeviceFound += DeviceFound;
            NatUtility.DeviceLost += DeviceLost;
            NatUtility.StartDiscovery();

            return true;
        }

        #region NAT

        private async void DeviceFound(object sender, DeviceEventArgs args)
        {
            try
            {
                INatDevice device = args.Device;

                Mapping mapping = new Mapping(Protocol, Port, Port);
                await device.CreatePortMapAsync(mapping);
            }
            catch (Exception ex)
            {
                InitResult = false;
                _socket.Dispose();
                Debug.LogError(ex.ToString());
            }
            _uPnPLocker.Set();
        }

        private void DeviceLost(object sender, DeviceEventArgs args)
        {
            _socket.Dispose();
            InitResult = false;
            Debug.LogError("Router device is lost");
            _uPnPLocker.Set();
        }

        #endregion


        private void FixedUpdate()
        {
            while (_actions.TryDequeue(out Action action))
            {
                action.Invoke();
            }
        }

        private void OnDestroy()
        {
            Destroy();
        }

        public void Destroy()
        {
            if (_socket == null)
                return;
            try
            {
                _socket.Close();
                _socket.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void OnApplicationQuit()
        {
            NatUtility.StopDiscovery();
        }

    }
}
