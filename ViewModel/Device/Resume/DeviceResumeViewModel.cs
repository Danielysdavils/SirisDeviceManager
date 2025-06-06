using SirisDeviceManager.Base;
using SirisDeviceManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
            set
            {
                if (_logs != null)
                    _logs.CollectionChanged -= Messages_CollectionChanged;

                _logs = value;

                if (_logs != null)
                    _logs.CollectionChanged += Messages_CollectionChanged;

                OnPropertyChanged();
                OnPropertyChanged(nameof(AllMessages));
            }
        }

        public string AllMessages => string.Join("\n", Logs);

        public ICommand DeleteLogsCommand { get; set; }
        public ICommand UpdateDeviceCommand { get; set; }
        public ICommand RebootDeviceCommand { get; set; }
        public ICommand ReestartRaspRunCommand { get; set; }


        public DeviceResumeViewModel()
        {
            Logs.CollectionChanged += (s, e) => OnPropertyChanged(nameof(AllMessages));

            DeleteLogsCommand = new RelayCommand<object>(_ => DeleteLogs());
            UpdateDeviceCommand = new RelayCommand<object>(_ =>  UpdateDevice());
            RebootDeviceCommand = new RelayCommand<object>(_ =>  RebootDevice());
            ReestartRaspRunCommand = new RelayCommand<object>(_ => ReestartRaspRun()); 
        }

        public void UpdateDevice(SirisDeviceManager.Model.Device device)
        {
            SerialNumber = device.SerialNumber;
            Name = device.Name;
            Ip = device.Ip;
            Name = device.Name;
            UpdateLogs(device);
        }

        private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(AllMessages));
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

        public async void UpdateDevice()
        {
            List<SirisDeviceManager.Model.Device> listDev = new();
            SirisDeviceManager.Model.Device? d = AppSessionManager.Instance.GetDevicebyId(SerialNumber);
            if(d != null)
                listDev.Add(d);

            await SshService.Instance.UpdateAllDevices(listDev);
        }

        public async void RebootDevice()
        {
            List<SirisDeviceManager.Model.Device> listDev = new();
            SirisDeviceManager.Model.Device? d = AppSessionManager.Instance.GetDevicebyId(SerialNumber);
            if (d != null)
                listDev.Add(d);

            await SshService.Instance.ExecuteSSHCommand(listDev, "sudo reboot");
        }

        public async void ReestartRaspRun()
        {
            List<SirisDeviceManager.Model.Device> listDev = new();
            SirisDeviceManager.Model.Device? d = AppSessionManager.Instance.GetDevicebyId(SerialNumber);
            if (d != null)
                listDev.Add(d);

            await SshService.Instance.ExecuteSSHCommand(listDev, "systemctl --user restart rasprun.service");
        }

    }
}
