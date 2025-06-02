using SirisDeviceManager.Base;
using SirisDeviceManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SirisDeviceManager.ViewModel.Device.Resume
{
    public class DeviceResumeViewModel : BaseViewModel
    {
        private string _serialNumber = string.Empty;
        public string SerialNumber
        {
            get => _serialNumber;
            set => SetProperty(ref _serialNumber, value);
        }

        private string _ip = string.Empty;
        public string Ip
        {
            get => _ip;
            set => SetProperty(ref _ip, value);
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private ObservableCollection<string> _logs = new();
        public ObservableCollection<string> Logs
        {
            get => _logs;
            set => SetProperty(ref _logs, value);
        }

        public ICommand DeleteLogsCommand { get; set; }
        public DeviceResumeViewModel()
        {
            DeleteLogsCommand = new RelayCommand<object>(_ => DeleteLogs());
        }

        public void UpdateDevice(SirisDeviceManager.Model.Device device)
        {
            SerialNumber = device.SerialNumber;
            Ip = device.Ip;
            Name = device.Name;
            UpdateLogs(device);
        }

        public void UpdateLogs(SirisDeviceManager.Model.Device device)
        {
            Logs = new ObservableCollection<string>(device.Logs);
        }

        public void DeleteLogs()
        {
            AppSessionManager.Instance.RemoveLogs(SerialNumber);
            Logs = new ObservableCollection<string>();
        }

    }
}
