using Microsoft.Identity.Client;
using SirisDeviceManager.Base;
using SirisDeviceManager.Model;
using SirisDeviceManager.Services;
using SirisDeviceManager.View.UserControls.Device.Resume;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.ViewModel.Device.Panel.DeviceList
{
    public class DeviceItemViewModel : BaseViewModel
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

        private string _version = "v0.0";
        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);    
        }

        private string _sessionState = "Any";
        public string SessionState
        {
            get => _sessionState;
            set => SetProperty(ref _sessionState, value);
        }

        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        private string _downloadState = "Any download";
        public string DownloadState
        {
            get => _downloadState;
            set => SetProperty(ref _downloadState, value);
        }

        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);    
        }

        public DeviceItemViewModel(SirisDeviceManager.Model.Device device)
        {
            UpdateDevice(device);
        }

        public void UpdateDevice(SirisDeviceManager.Model.Device device)
        {
            SerialNumber = device.SerialNumber;
            Ip = device.Ip;
            Version = device.Version;
            IsConnected = device.IsConnected;
            IsRunning = device.SessionState == Model.SessionState.SESSION_EXIST_RUNNING_FILES || device.SessionState == Model.SessionState.SESSION_EXIST_RUNNING_STREAM;
            SessionState = GetState(device.SessionState);
            DownloadState = GetDownloadState(device.DownloadState);
        }

        private static string GetState(SessionState state)
        {
            if (state == Model.SessionState.SESSION_ANY)
                return "🕳️ Any session";

            if (state == Model.SessionState.SESSION_EXIST_WAITING)
                return "⏳ Waiting session...";

            if (state == Model.SessionState.SESSION_EXIST_RUNNING_FILES)
                return "▶️ Playing files!";

            if (state == Model.SessionState.SESSION_EXIST_RUNNING_STREAM)
                return "▶️ Streaming video!";

            return "";
        }

        private static string GetDownloadState(DownloadState state)
        {
            if (state == Model.DownloadState.DOWNLOADING)
                return "⏳ Download...";

            if (state == Model.DownloadState.DOWNLOAD_ANY)
                return "🕳️ Any download";

            if (state == Model.DownloadState.DOWNLOAD_ERROR)
                return "🚫 ERROR in download";

            if (state == Model.DownloadState.DOWLOAD_COMPLEATED)
                return "✅ Download completed!";

            return "";
        }
    }
}
