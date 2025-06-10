using SirisDeviceManager.Base;
using SirisDeviceManager.Services;
using SirisDeviceManager.ViewModel.Device.Panel;
using SirisDeviceManager.ViewModel.Device.Resume;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SirisDeviceManager.ViewModel.Device
{
    public class DeviceViewModel : BaseViewModel
    {
        public DevicePanelViewModel DevicePanelViewModel { get; set; }
        public DeviceResumeViewModel DeviceResumeViewModel { get; set; }

        private System.Timers.Timer _updateTimer;
        private SirisDeviceManager.Model.Device? _selectedDevice;


        public DeviceViewModel()
        {
            DevicePanelViewModel = new();
            DeviceResumeViewModel = new();

            _updateTimer = new System.Timers.Timer();
            _updateTimer.Elapsed += UpdateDeviceLogs;
            _updateTimer.AutoReset = true;

            DevicePanelViewModel.DeviceListViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(DevicePanelViewModel.DeviceListViewModel.SelectedDevice))
                {
                    SirisDeviceManager.Model.Device? selectedDevice = DevicePanelViewModel.DeviceListViewModel.SelectedDevice;
                    if (selectedDevice != null)
                    {
                        selectedDevice = AppSessionManager.Instance.GetDevicebyId(selectedDevice.SerialNumber);
                        DeviceResumeViewModel.UpdateDevice(selectedDevice!);
                        StartDeviceUpdateTimer(selectedDevice!);
                    }
                    else
                    {
                        StopDeviceUpdateTimer();
                    }
                }
            };
        }

        private void StartDeviceUpdateTimer(SirisDeviceManager.Model.Device dev)
        {
            _selectedDevice = dev;
            _updateTimer.Stop();

            _updateTimer.Interval = 1000;
            _updateTimer.Start();
        }

        private void StopDeviceUpdateTimer()
        {
            _updateTimer.Stop();
            _selectedDevice = null;
        }

        private void UpdateDeviceLogs(object? sender, ElapsedEventArgs e)
        {
            if (_selectedDevice != null)
            {
                var updateDevice = AppSessionManager.Instance.GetDevicebyId(_selectedDevice.SerialNumber);

                if (updateDevice != null)
                    DeviceResumeViewModel.UpdateLogs(updateDevice);
            }
        }

        public void LoadDevices()
        {
            DevicePanelViewModel.LoadDevices();
        }
    }
}
