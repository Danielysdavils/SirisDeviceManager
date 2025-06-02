using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.Model
{
    public class Network
    {
        public string Gateway { get; set; } = string.Empty;
        public string Netmask { get; set; } = string.Empty;
        public string Dns { get; set; } = string.Empty;
    }
}
