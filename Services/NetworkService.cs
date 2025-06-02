using SirisDeviceManager.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.Services
{
    public class NetworkService
    {
        public NetworkService() { }

        public Network LoadNetwork()
        {
            try
            {
                string networkFilePath = Path.Combine(AppContext.BaseDirectory, "Properties", "network.txt");
                List<string> lines = File
                    .ReadAllLines(networkFilePath)
                    .ToList();

                return new()
                {
                    Gateway = lines[0],
                    Netmask = lines[1],
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new();
            }
        }
    }
}
