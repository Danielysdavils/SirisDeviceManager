using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.Model
{
    public class Server
    {
        public string Ip { get; set; } = string.Empty;
        public int Port { get; set; } = -1;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
