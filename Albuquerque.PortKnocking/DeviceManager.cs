using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;


namespace Albuquerque.PortKnocking
{
    class DeviceManager
    {
        public static ICaptureDevice GetInterfaceByIndex(int index)
        {
            var devices = CaptureDeviceList.Instance;
            if (devices.Count < 1)
            {
                throw new Exception("Interface inválida");
            }
            return devices[index];

        }

        public static Dictionary<string, string> GetDevices()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            var devices = CaptureDeviceList.Instance;

            if (devices.Count < 1)
            {
                throw new Exception("Nenhuma interface de rede disponível");
            }

            foreach (var dev in devices)
            {
                result.Add(dev.Name, dev.Description.ToUpper());
            }

            return result;
        }
    }
}
