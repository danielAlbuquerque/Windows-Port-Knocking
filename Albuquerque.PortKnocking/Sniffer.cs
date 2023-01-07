using PacketDotNet;
using SharpPcap;
using System;
using System.Collections;


namespace Albuquerque.PortKnocking
{
    class Sniffer
    {

        PortKnocker _portKnocker;
        string _execCommand;
        string _protocol;

        public Sniffer(PortKnocker portKnocker, string execCommand, string protocol)
        {
            _portKnocker = portKnocker;
            _execCommand = execCommand;
            _protocol = protocol;
        }

        public static string GenerateFilterString(IEnumerable ports, string protocol)
        { 
            string result = "";
            foreach (string port in ports)
            {
                result += $"{protocol} dst port {port} or ";
            }
            return result.Substring(0, result.Length - 4);
        }

        public void Sniff(ICaptureDevice device, string filter)
        {
            device.OnPacketArrival +=
                new PacketArrivalEventHandler(Device_OnPacketArrival);

            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceModes.Promiscuous, readTimeoutMilliseconds);

            device.Filter = filter;
            // Start capture packets
            device.Capture();

        }

        private void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            ushort destinationPort;
            if (_protocol.ToUpper() == "TCP")
                destinationPort = e.GetPacket().GetPacket().Extract<TcpPacket>().DestinationPort;
            else
                destinationPort = e.GetPacket().GetPacket().Extract<UdpPacket>().DestinationPort;
            Console.WriteLine(destinationPort);
            if (_portKnocker.Check(destinationPort))
            {
                Commander.RunCommand(_execCommand, false);
            }
        }

    }
}
