//using Mono.Nat;
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
    public class INetworkNode : MonoBehaviour
    {
        public delegate void CommandHandle(INodePlayer player, NodeInputPacketBuffer buffer);

        protected Socket _socket;

        protected readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

        public int MyPlayerId { get; internal set; }

        protected readonly Dictionary<byte, CommandHandle> _commands = new Dictionary<byte, CommandHandle>();

        public void AddPacketHandle(byte id, CommandHandle handle)
        {
            if (_commands.ContainsKey(id))
            {
                Debug.LogError($"Node network: (AddPacketHandle) packet {id} already exist, be removed and append new");
                _commands.Remove(id);
            }

            _commands.Add(id, handle);
        }

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
            var s = _socket;
            _socket = null;
            try
            {
                s.Close();
                s.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void OnApplicationQuit()
        {
            //NatUtility.StopDiscovery();
        }

    }

    public class INetworkNode<T> : INetworkNode
        where T : INodePlayer
    {
#if DEBUG
        public event CommandHandle OnReceivePacket;

        protected void AppendOnReceiveData(T player, NodeInputPacketBuffer packet)
        {
            _actions.Enqueue(() => { OnReceivePacket?.Invoke(player, packet); });
        }
#endif


        protected ManualResetEvent _uPnPLocker = new ManualResetEvent(false);

        protected bool InitResult { get; set; } = false;

        public int Port { get; protected set; }

        //protected Protocol Protocol { get; set; }

        protected readonly ConcurrentDictionary<int, T> _players = new ConcurrentDictionary<int, T>();

        //protected Mapping Mapping { get; set; }

        public string UpNpDescription { get; set; }

        public virtual bool Initiliaze(string ip, ref int port, int myPlayerId)
        {
            MyPlayerId = myPlayerId;

            //NatUtility.DeviceFound += DeviceFound;
            //NatUtility.DeviceLost += DeviceLost;
            //NatUtility.StartDiscovery();

            _uPnPLocker.Set();
            return true;
        }

        #region NAT

        //private async void DeviceFound(object sender, DeviceEventArgs args)
        //{
        //    try
        //    {
        //        INatDevice device = args.Device;

        //        Mapping = new Mapping(Protocol, Port, Port) { Description = UpNpDescription };
        //        await device.CreatePortMapAsync(Mapping);
        //    }
        //    catch (Exception ex)
        //    {
        //        InitResult = false;
        //        //_socket?.Dispose();
        //        Debug.LogError(ex.ToString());
        //    }
        //    _uPnPLocker.Set();
        //}

        //private async void DeviceLost(object sender, DeviceEventArgs args)
        //{
        //    if (Mapping != null)
        //    {
        //        INatDevice device = args.Device;
        //        await device.DeletePortMapAsync(Mapping);
        //    }
        //    //_socket?.Dispose();
        //    InitResult = false;
        //    Debug.LogError("Router device is lost");
        //    _uPnPLocker?.Set();
        //}

        #endregion

        #region Proxy

        public void SetProxyServerData()
        { 
        
        }

        #endregion

    }
}
