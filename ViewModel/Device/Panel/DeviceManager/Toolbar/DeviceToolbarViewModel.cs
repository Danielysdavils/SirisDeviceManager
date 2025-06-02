using SirisDeviceManager.Base;
using SirisDeviceManager.Services;
using SirisDeviceManager.View.UserControls.Device.Panel.DeviceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SirisDeviceManager.ViewModel.Device.Panel.DeviceManager.Toolbar
{
    public class DeviceToolbarViewModel : BaseViewModel
    {
        private readonly DeviceManagerViewModel _viewModel;

        public DeviceToolbarViewModel(DeviceManagerViewModel viewModel)
        {
            _viewModel = viewModel;

            //commands
            LoadDevicesCommand = new RelayCommand<object>(_ => LoadDevices());
            ExecutebySudoCommand = new RelayCommand<object>(_ => ExecutebySudo());
            UpdateDevicesCommand = new RelayCommand<object>(_ => UpdateDevices());
        }

        public ICommand LoadDevicesCommand { get; }
        public ICommand UpdateDevicesCommand { get; }
        public ICommand ExecutebySudoCommand { get; }

        //functions
        public void LoadDevices()
        {
            _viewModel.LoadDevices();
        }

        public void UpdateDevices()
        {
            _viewModel.ShowModal(new ExecuteUpdateModal());
        }

        public void ExecutebySudo()
        {
            _viewModel.ShowModal(new ExecutebySudoModal());
        }
    }
}
