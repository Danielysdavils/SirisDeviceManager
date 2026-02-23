using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.Services
{
    public sealed class DeviceUpdatingService
    {
        private static readonly Lazy<DeviceUpdatingService> _instance = 
            new Lazy<DeviceUpdatingService>(() => new DeviceUpdatingService());

        private LoggerService _loggerService { get; set; }
        private List<SirisDeviceManager.Model.Device> _devices;

        private CancellationTokenSource _cancellationTokenSource;
        public static DeviceUpdatingService Instance = _instance.Value; 


        public DeviceUpdatingService()
        {
            _loggerService = new();
            _devices = new();
            _cancellationTokenSource = new();
        }


        public void StartGettingLogs()
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
                _cancellationTokenSource = new();

            Task.Run(() => ReceiveLogsAsync(_cancellationTokenSource.Token));
        }

        public void StopGettingLogs()
        {
            _cancellationTokenSource.Cancel();
        }

        private async Task ReceiveLogsAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    _devices = AppSessionManager.Instance.GetDevices();
                    Console.WriteLine(_devices.Count);
                    var task = new List<Task>();

                    foreach (var device in _devices)
                    {
                        try
                        {
                            task.Add(Task.Run(() => _loggerService.GetDeviceLog(device), token));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }

                    await Task.WhenAll(task);
                    await Task.Delay(TimeSpan.FromSeconds(3), token);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
