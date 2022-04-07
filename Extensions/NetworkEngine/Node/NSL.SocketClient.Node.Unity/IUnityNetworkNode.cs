using System;
using UnityEngine;

namespace SCL.Node.Unity
{
    public class IUnityNetworkNode : MonoBehaviour
    {
        public System.Net.Sockets.Socket _socket { get; set; }

        private void OnDestroy()
        {
            Destroy();
        }

        //todo: fix
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
}
