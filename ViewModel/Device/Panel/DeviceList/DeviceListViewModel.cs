using SirisDeviceManager.Base;
using SirisDeviceManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SirisDeviceManager.ViewModel.Device.Panel.DeviceList
{
    public class DeviceListViewModel : BaseViewModel
    {
        private DeviceService _deviceService;

        public ObservableCollection<DeviceItemViewModel> Devices { get; set; } = new();

        public ICommand SelectedDeviceCommand { get; set; }

        
        private SirisDeviceManager.Model.Device? _selectedDevice;
        public SirisDeviceManager.Model.Device? SelectedDevice
        {
            get => _selectedDevice;
            set => SetProperty(ref _selectedDevice, value);
        }


        public DeviceListViewModel()
        {
            _deviceService = new DeviceService();
            SelectedDeviceCommand = new RelayCommand<DeviceItemViewModel>(UpdateSelectedDevice);
            AppSessionManager.Instance.DeviceUpdate += onDeviceUpdated;
        }


        private void UpdateSelectedDevice(DeviceItemViewModel device)
        {
            SelectedDevice = AppSessionManager.Instance.GetDevicebyId(device.SerialNumber);
        }

        public void onDeviceUpdated(SirisDeviceManager.Model.Device device)
        {
            var item = Devices.FirstOrDefault(d => d.SerialNumber == device.SerialNumber);
            item?.UpdateDevice(device);
        }

        public async Task Load()
        {
            try
            {
                await AppSessionManager.Instance.LoadDevicesAsync();
                var devices = CreateItems(AppSessionManager.Instance.GetDevices());

                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    Devices.Clear();
                    foreach (DeviceItemViewModel dev in devices)
                        Devices.Add(dev);
                });

            }catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private List<DeviceItemViewModel> CreateItems(List<SirisDeviceManager.Model.Device> devices)
        {
            List<DeviceItemViewModel> items = new();

            foreach(var dev in devices)
                items.Add(new(dev));

            return items;
        }
    }
}
