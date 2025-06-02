using SirisDeviceManager.Base;
using SirisDeviceManager.Services;
using SirisDeviceManager.ViewModel.Device;
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
            NETWORK = 2,
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }

        public DeviceViewModel DeviceViewModel { get; set; }

        public SirisDeviceManagerViewModel()
        {
            DeviceViewModel = new();
            SelectedIndex = 1;

            Init();
        }

        private async void Init()
        {
            try
            {
                AppSessionManager.Instance.LoadNetwork();

                await AppSessionManager.Instance.LoadDevicesAsync();

                DeviceUpdatingService.Instance.StartGettingLogs();
                Thread.Sleep(1000);

                DeviceViewModel.LoadDevices();

            }catch(Exception ex)
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
