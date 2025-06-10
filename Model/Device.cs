using SiRISApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.Model
{
    public class Device
    {
        public string Ip { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string User { get; set; } = "pi";
        public string Password { get; set; } = "1234";
        public DateTime DateTime { get; set; } = DateTime.Now;
        public bool IsConnected { get; set; } = false;
        public string Version { get; set; } = string.Empty;
        public List<string> Logs { get; set; } = new();


        //Reboot
        public Reboot Reboot { get; set; } = new();

        //Session
        public Session Session { get; set; } = new();

        //Download
        public Download Download { get; set; } = new();
    }
}
