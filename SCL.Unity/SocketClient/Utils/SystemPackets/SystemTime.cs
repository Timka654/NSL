using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCL.SocketClient;
using SCL.SocketClient.Utils.Buffer;

namespace SCL.SocketClient.Utils.SystemPackets
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
            catch (Exception ex)
            {
                //ThreadHelper.InvokeOnMain(() => { UnityEngine.Debug.Log($"{now} - {dt} =  {now - dt}"); });
            }
        }
        
    }
}
