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
    public class ExecuteUpdateModalViewModel : BaseViewModel
    {
        private string _state = string.Empty;
        public string State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }


        public ICommand UpdateCommand { get; set; }
        public ExecuteUpdateModalViewModel()
        {
            UpdateCommand = new RelayCommand<object>(_ => Update());
            MessageService.Instance.StatusChanged += OnStatusChange;
        }
        private void OnStatusChange(string state)
        {
            State = state;
        }

        public async void Update()
        {
            try
            {
                await SshService.Instance.UpdateAllDevices(AppSessionManager.Instance.GetDevices());
            }
            catch { }
        }
    }
}
