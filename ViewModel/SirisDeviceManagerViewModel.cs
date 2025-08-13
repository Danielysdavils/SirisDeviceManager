using SirisDeviceManager.Base;
using SirisDeviceManager.Services;
using SirisDeviceManager.ViewModel.Device;
using SirisDeviceManager.ViewModel.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SirisDeviceManager.ViewModel
{
    public class SirisDeviceManagerViewModel : BaseViewModel
    {
        public enum SIRISMANAGER_INDEX
        {
            LOGIN = 0,
            DEVICE = 1,
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }

        public DeviceViewModel DeviceViewModel { get; set; }
        public LoginViewModel LoginViewModel { get; set; }

        public SirisDeviceManagerViewModel()
        {
            DeviceViewModel = new();
            LoginViewModel = new(this);

            SelectedIndex = 0;

            LoginViewModel.Authenticated += (s, e) => Init();
            PageService.Instance.ChangeAppIndexEvent += ChangePage;
        }

        private async void Init()
        {
            try
            {
                AppSessionManager.Instance.LoadNetwork();

                await AppSessionManager.Instance.LoadDevicesAsync();

                DeviceUpdatingService.Instance.StartGettingLogs();
                await Task.Delay(1000);

                DeviceViewModel.LoadDevices();

                SelectedIndex = (int)SIRISMANAGER_INDEX.DEVICE;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ChangePage(object? sender, EventArgs e)
        {
            PageService? s = (PageService?)sender;
            if (s != null)
                SelectedIndex = s.AppIndex;
        }

    }
}
