using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.Model
{
    public enum SessionState
    {
        SESSION_ANY,
        SESSION_EXIST_WAITING,
        SESSION_EXIST_RUNNING_FILES,
        SESSION_EXIST_RUNNING_STREAM
    }

    public enum DownloadState
    {
        DOWNLOAD_ANY,
        DOWNLOADING,
        DOWNLOAD_ERROR,
        DOWLOAD_COMPLEATED
    }

    public class Device
    {
        public string Ip { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string User { get; set; } = "pi";
        public string Password { get; set; } = "1234";
        public DateTime DateTime { get; set; } = DateTime.Now;
        
        public bool IsConnected { get; set; } = false;

        public SessionState SessionState { get; set; } = SessionState.SESSION_ANY;
        public DownloadState DownloadState { get; set; } = DownloadState.DOWNLOAD_ANY;

        public string Version { get; set; } = string.Empty;
        public List<string> Logs { get; set; } = new();
    }
}
