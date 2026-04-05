using NSL.Extensions.NAT.uPnP;
using NSL.Extensions.NAT.uPnP.Enums;
using NSL.Extensions.NAT.uPnP.EventArgs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Extensions.NAT
{
    public class UPNPProvider
    {
        static readonly object _lock = new object();
        static readonly List<INatDevice> DeviceList = new List<INatDevice>();
        static readonly List<Mapping> MapList = new List<Mapping>();

        static Timer checkTimer;

        public static List<INatDevice> GetDevices()
        {
            lock (_lock)
                return DeviceList.ToList();
        }

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

            lock (_lock)
                MapList.Add(mapping);

            ProcessAppendMapping(mapping);

            return true;
        }

        public static void RemoveMapping(Func<Mapping, bool> func)
        {
            Mapping[] toRemove;
            lock (_lock)
            {
                toRemove = MapList.Where(func).ToArray();
                foreach (var item in toRemove)
                    MapList.Remove(item);
            }

            foreach (var item in toRemove)
                _ = RemoveMappingAsync(item);
        }

        public static Task RemoveMappingAsync(Mapping mapping)
        {
            INatDevice[] devices;
            lock (_lock)
                devices = DeviceList.ToArray();

            return Task.WhenAll(devices.Select(d => d.DeletePortMapAsync(mapping)));
        }

        private static void ProcessAppendMapping(Mapping map)
        {
            INatDevice[] devices;
            lock (_lock)
                devices = DeviceList.ToArray();

            foreach (var device in devices)
                _ = ProcessDeviceMappingAsync(device, map);
        }

        private static void ProcessAppendDevice(INatDevice device)
        {
            Mapping[] mappings;
            lock (_lock)
                mappings = MapList.ToArray();

            foreach (var item in mappings)
                _ = ProcessDeviceMappingAsync(device, item);
        }

        private static async Task ProcessDeviceMappingAsync(INatDevice device, Mapping mapping)
        {
            try
            {
                await device.CreatePortMapAsync(mapping);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UPNPProvider] CreatePortMap failed: {ex.Message}");
            }
        }

        private static void DeviceFound(object sender, DeviceEventArgs args)
        {
            lock (_lock)
                DeviceList.Add(args.Device);

            ProcessAppendDevice(args.Device);
        }

        private static void DeviceLost(object sender, DeviceEventArgs args)
        {
            lock (_lock)
                DeviceList.Remove(args.Device);
        }
    }
}
