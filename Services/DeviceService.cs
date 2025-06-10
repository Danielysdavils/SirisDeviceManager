using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SirisDeviceManager.Model;

namespace SirisDeviceManager.Services
{
    public class DeviceService
    {
        public DeviceService() { }
        
        private readonly SiRISApi.Repository.Context _context = new();
        public async Task<List<Device>> GetActiveDevices(Network net)
        {
            List<Device> localDevices = new();

            try
            {
                List<SiRISApi.Models.ReceiverEquipment> devicesActivos = await _context.ReceiverEquipments
                    .ToListAsync();

                if(devicesActivos.Count <= 0) 
                    return localDevices;
               
                devicesActivos.ForEach(d => localDevices.Add(CreateDevice(d)));
                return localDevices;

            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return localDevices;
            }
        }

        public async Task<List<Device>> ScanActiveDevices(List<Device> devices, Network net)
        {
            List<string> networkIps = await ScaneNetworkAsyn(net);
            if (networkIps.Count <= 0)
                return devices;

            return devices.Where(d => networkIps.Contains(d.Ip)).ToList();
        }

        private static async Task<List<string>> ScaneNetworkAsyn(Network network)
        {
            var activeIps = new ConcurrentBag<string>();

            string baseIp = string.Join(".", network.Gateway.Split(".").Take(3)) + ".";
            var tasks = new List<Task>();

            for(int i = 1; i < 255; i++)
            {
                string ip = baseIp + i;
                tasks.Add(Task.Run(async () =>
                {
                    using Ping ping = new Ping();
                    try
                    {
                        PingReply reply = await ping.SendPingAsync(ip, 100);
                        if (reply.Status == IPStatus.Success)
                            activeIps.Add(ip);
                    }
                    catch { }
                }));
            }

            await Task.WhenAll(tasks);
            return activeIps.ToList();
        }
    
        private static Device CreateDevice(SiRISApi.Models.ReceiverEquipment equipment)
        {
            return new()
            {
                Ip = equipment.Ip,
                Name = equipment.Name,
                DateTime = equipment.StatusDateTime,
                SerialNumber = equipment.SerialNumber,
            };
        } 
    }
}
