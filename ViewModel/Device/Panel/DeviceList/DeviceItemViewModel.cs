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
using System.Windows.Threading;

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

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);   
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

        private string _sessionState = "Nenhuma sessão";
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

        private bool _isDownloading = false;
        public bool IsDownloading
        {
            get => _isDownloading;
            set => SetProperty(ref _isDownloading, value);
        }

        private string _downloadState = "Nenhum download";
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

        private string _rebootStatus = string.Empty;
        public string RebootStatus
        {
            get => _rebootStatus;
            set => SetProperty(ref _rebootStatus, value);
        }

        private TimeSpan _rebootCountdown = TimeSpan.Zero;
        private DispatcherTimer? _countdownTimer;

        public DeviceItemViewModel(SirisDeviceManager.Model.Device device)
        {
            UpdateDevice(device);
        }

        public void UpdateDevice(SirisDeviceManager.Model.Device device)
        {
            SerialNumber = device.SerialNumber;
            Name = device.Name;
            Ip = device.Ip;
            Version = device.Version;
            IsConnected = device.IsConnected;
            IsRunning = device.IsRunning;
            IsDownloading = device.DownloadState == Model.DownloadState.DOWNLOADING ? true : false;
            SessionState = GetState(device.SessionState, device.SessionId);
            DownloadState = GetDownloadState(device.DownloadState);

            UpdateRebootState(device.IsRebootScheduled, device.RebootCountDown);
        }

        private void UpdateRebootState(bool isSchedule, TimeSpan countdown)
        {
            if(isSchedule)
            {
                _rebootCountdown = countdown;
                StartCountdown();
            }
            else
            {
                StopCountdown();
                RebootStatus = string.Empty;
            }
        }

        private void StartCountdown()
        {
            if (_countdownTimer == null)
            {
                _countdownTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                _countdownTimer.Tick += CountdownTimer_Tick;
            }

            UpdateRebootStatusText();
            _countdownTimer.Start();
        }

        private void StopCountdown()
        {
            _countdownTimer?.Stop();
            _rebootCountdown = TimeSpan.Zero;
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            if (_rebootCountdown > TimeSpan.Zero)
            {
                _rebootCountdown = _rebootCountdown.Subtract(TimeSpan.FromSeconds(1));
                UpdateRebootStatusText();
            }
            else
            {
                StopCountdown();
                RebootStatus = string.Empty;
            }
        }

        private void UpdateRebootStatusText()
        {
            RebootStatus = $"[🚨REBOOT] {_rebootCountdown:mm\\:ss}";
        }

        private static string GetState(SessionState state, string sessionId)
        {
            if (state == Model.SessionState.SESSION_ANY)
                return "🕳️ Nenhuma sessão";

            if (state == Model.SessionState.SESSION_EXIST_WAITING)
                return "⏳ Aguardando a sessão...";

            if (state == Model.SessionState.SESSION_EXIST_RUNNING_FILES)
                return $"▶️ [{sessionId}] Reproduzindo arquivos!";

            if (state == Model.SessionState.SESSION_EXIST_RUNNING_STREAM)
                return $"▶️ [{sessionId}] Reproduzindo transmissão!";

            return "";
        }

        private static string GetDownloadState(DownloadState state)
        {
            if (state == Model.DownloadState.DOWNLOADING)
                return "⏳ Baixando...";

            if (state == Model.DownloadState.DOWNLOAD_ANY)
                return "🕳️ Nenhum download";

            if (state == Model.DownloadState.DOWNLOAD_ERROR)
                return "🚫 ERROR ao baixar";

            if (state == Model.DownloadState.DOWLOAD_COMPLEATED)
                return "✅ Download completed!";

            return "";
        }
    }
}
