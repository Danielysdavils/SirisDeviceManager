using SirisDeviceManager.Base;
using SirisDeviceManager.ViewModel.Device.Panel.DeviceManager.Modal;
using SirisDeviceManager.ViewModel.Device.Panel.DeviceManager.Toolbar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.ViewModel.Device.Panel.DeviceManager
{
    public class DeviceManagerViewModel : BaseViewModel
    {
        private object? _currentModal;
        public object? CurrentModal
        {
            get => _currentModal;
            set => SetProperty(ref _currentModal, value);
        }

        private bool _isModalOpen;
        public bool IsModalOpen
        {
            get => _isModalOpen;
            set => SetProperty(ref _isModalOpen, value);
        }

        public DevicePanelViewModel DevicePanelViewModel { get; set; }
        public ExecutebySudoModalViewModel ExecutebySudoModalViewModel { get; set; }
        public ExecuteUpdateModalViewModel ExecuteUpdateModal { get; set; }
        public DeviceToolbarViewModel DeviceToolbarViewModel { get; set; }

        public DeviceManagerViewModel(DevicePanelViewModel devicePanelViewModel)
        {
            DeviceToolbarViewModel = new(this);
            ExecuteUpdateModal = new();
            ExecutebySudoModalViewModel = new();

            DevicePanelViewModel = devicePanelViewModel;
        }

        public void ShowModal(object modal)
        {
            CurrentModal = modal;
            IsModalOpen = true;
        }

        public void LoadDevices()
        {
            DevicePanelViewModel.LoadDevices();
        }
    }
}
