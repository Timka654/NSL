using Mono.Nat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NSL.Extensions.NAT
{
    public class UPNPProvider
    {
        static List<INatDevice> DeviceList = new List<INatDevice>();

        static List<Mapping> MapList = new List<Mapping>();

        static Timer checkTimer;

        public static List<INatDevice> GetDevices() => DeviceList;

        static UPNPProvider()
        {
            NatUtility.DeviceFound += DeviceFound;

            NatUtility.DeviceLost += DeviceLost;

            NatUtility.StartDiscovery();

            checkTimer = new Timer(CheckTick);

            checkTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15));
        }

        private static void CheckTick(object state) => NatUtility.StartDiscovery();

        public static bool AddMapping(Protocol protocol, int port, int? publicPort = null, string description = default)
        {
            var mapping = new Mapping(protocol, port, publicPort ?? port) { Description = description };

            MapList.Add(mapping);

            if (!MapList.Any())
                return false;

            ProcessAppendMapping(mapping);

            return true;
        }

        public static void RemoveMapping(Func<Mapping, bool> func)
        {
            foreach (var item in MapList.Where(x=>func(x)).ToArray())
            {
                RemoveMapping(item);
            }
        }

        public static async void RemoveMapping(Mapping mapping)
        {
            foreach (var device in DeviceList)
            {
                await device.DeletePortMapAsync(mapping);
            }
        }

        private static void ProcessAppendMapping(Mapping map)
        {
            foreach (var device in DeviceList.ToArray())
            {
                ProcessDeviceMapping(device, map);
            }
        }

        private static void ProcessAppendDevice(INatDevice device)
        {
            foreach (var item in MapList.ToArray())
            {
                ProcessDeviceMapping(device, item);
            }
        }

        private static async void ProcessDeviceMapping(INatDevice device, Mapping mapping)
        {
            try
            {
                await device.CreatePortMapAsync(mapping);
            }
#if DEBUG
            catch (Exception ex)
            {
            }
#else
            catch { }
#endif
        }

        private static void DeviceFound(object sender, DeviceEventArgs args)
        {
            DeviceList.Add(args.Device);
            ProcessAppendDevice(args.Device);
        }

        private static void DeviceLost(object sender, DeviceEventArgs args)
        {
            DeviceList.Remove(args.Device);
        }
    }
}
