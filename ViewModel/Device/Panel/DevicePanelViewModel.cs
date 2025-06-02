using SirisDeviceManager.Base;
using SirisDeviceManager.Services;
using SirisDeviceManager.ViewModel.Device.Panel.DeviceList;
using SirisDeviceManager.ViewModel.Device.Panel.DeviceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.ViewModel.Device.Panel
{
    public class DevicePanelViewModel : BaseViewModel
    {

        private int _qtdDevices = 0;
        public int QtdDevices
        {
            get => _qtdDevices;
            set => SetProperty(ref _qtdDevices, value); 
        }

        public DeviceListViewModel DeviceListViewModel { get; set; }
        public DeviceManagerViewModel DeviceManagerViewModel { get; set; }

        public DevicePanelViewModel()
        {
            DeviceListViewModel = new();
            DeviceManagerViewModel = new(this);
        }

        public async void LoadDevices()
        {
            await DeviceListViewModel.Load();
            QtdDevices = AppSessionManager.Instance.GetDevices().Count;
        }
    }
}
