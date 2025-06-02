using SirisDeviceManager.Base;
using SirisDeviceManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SirisDeviceManager.ViewModel.Device.Panel.DeviceManager.Modal
{
    public class ExecutebySudoModalViewModel : BaseViewModel
    {
        private string _command = string.Empty;
        public string Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        private string _state = string.Empty;
        public string State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        public ICommand SendCommand { get; }
        
        public ExecutebySudoModalViewModel()
        {
            SendCommand = new RelayCommand<object>(_ => Send());
            MessageService.Instance.StatusChanged += OnStatusChange;
            
        }

        private void OnStatusChange(string state)
        {
            State = state;
        }

        public async void Send()
        {
            try
            {
                List<string> res = await SshService.Instance.ExecuteSSHCommand(AppSessionManager.Instance.GetDevices(), Command);
                Console.WriteLine($"{res.Count}");
            }
            catch { }
        }

        
    }
}
