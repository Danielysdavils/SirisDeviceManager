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
    public class UpdateAudioModalViewModel : BaseViewModel
    {
        private int _audioVol = 80;
        public int AudioVol
        {
            get => _audioVol;
            set => SetProperty(ref _audioVol, value);
        }

        private string _state = string.Empty;
        public string State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        public ICommand UpdateAudio { get; }

        public UpdateAudioModalViewModel()
        {
            UpdateAudio = new RelayCommand<object>(_ => Send());
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
                string command = $"amixer set Master {AudioVol}%";
                List<string> res = await SshService.Instance.ExecuteSSHCommand(AppSessionManager.Instance.GetDevices(), command);
                Console.WriteLine(res);

                command = "sudo alsactl store";
                res = await SshService.Instance.ExecuteSSHCommand(AppSessionManager.Instance.GetDevices(), command);
                Console.WriteLine(res);
            }
            catch { }
        }
    }
}
