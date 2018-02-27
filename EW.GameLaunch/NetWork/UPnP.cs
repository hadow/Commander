using System;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Open.Nat;
namespace EW.NetWork
{
    public enum UPnPStatus { Enabled,Disabled,NotSupported}
    public class UPnP
    {

        static NatDevice natDevice;
        static Mapping mapping;
        static bool initialized;

        public static IPAddress ExternalIP { get; private set; }

        public static UPnPStatus Status { get { return initialized ? natDevice != null ?
                    UPnPStatus.Enabled : UPnPStatus.NotSupported : UPnPStatus.Disabled; } }


        public static async Task DiscoverNatDevices(int timeout)
        {
            initialized = true;

            NatDiscoverer.TraceSource.Switch.Level = SourceLevels.Verbose;

            var natDiscoverer = new NatDiscoverer();
            var token = new CancellationTokenSource(timeout);
            natDevice = await natDiscoverer.DiscoverDeviceAsync(PortMapper.Upnp, token);
            try
            {
                ExternalIP = await natDevice.GetExternalIPAsync();
            }
            catch(Exception exp)
            {

            }


        }

        public static async Task ForwardPort(int listen,int external)
        {
            mapping = new Mapping(Protocol.Tcp, listen, external, "EastWood");
            try
            {
                await natDevice.CreatePortMapAsync(mapping);
            }
            catch(Exception exp)
            {

            }
        }

        public static async Task RemovePortForward()
        {
            try
            {
                await natDevice.DeletePortMapAsync(mapping);

            }
            catch(Exception exp)
            {

            }
        }
    }
}