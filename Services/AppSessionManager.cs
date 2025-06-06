using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.Services
{
    public class AppSessionManager
    {
        private static AppSessionManager? _instance;
        public static AppSessionManager Instance =>
            _instance ??= new AppSessionManager(
                new NetworkService(),
                new DeviceService(),
                new LoggerService()
            );

        private readonly AppSessionStorage _storage = AppSessionStorage.Instance;
        private readonly NetworkService _networkService;
        private readonly DeviceService _deviceService;
        private readonly LoggerService _loggerService;

        public event Action<SirisDeviceManager.Model.Device>? DeviceUpdate;

        public AppSessionManager( NetworkService networkService, DeviceService deviceService, LoggerService loggerService)
        {
            _networkService = networkService;
            _deviceService = deviceService;
            _loggerService = loggerService;
        }

        public void LoadNetwork()
        {
            var network = _networkService.LoadNetwork();
            _storage.Network = network;
        }

        public async Task LoadDevicesAsync()
        {
            _storage.Devices.Clear();

            var devices = await _deviceService.GetActiveDevices(_storage.Network);
            _storage.Devices = devices;
        }

        public List<SirisDeviceManager.Model.Device> GetDevices()
        {
            return _storage.Devices;
        }

        public SirisDeviceManager.Model.Device? GetDevicebyId(string serialNumber)
        {
            return _storage.Devices
                .Where(d => d.SerialNumber == serialNumber)
                .FirstOrDefault();
        }

        public async Task AddLog(string serialNumber, string log)
        {
            SirisDeviceManager.Model.Device? dev = GetDevicebyId(serialNumber);
            if(dev != null)
            {
                if (dev.Logs.Count >= 400)
                    dev.Logs.Clear();

                await _loggerService.ParseLog(dev, log);
                dev.Logs.Add(log);

                DeviceUpdate?.Invoke(dev);
            } 
        }

        public void RemoveLogs(string serialNumber)
        {
            SirisDeviceManager.Model.Device? dev = GetDevicebyId(serialNumber);
            if (dev != null)
                dev.Logs.Clear();
        }

    }
}
