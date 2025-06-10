using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.Model
{
    public enum DownloadState
    {
        DOWNLOAD_ANY,
        DOWNLOADING,
        DOWNLOAD_ERROR,
        DOWLOAD_COMPLEATED
    }

    public class Download
    {
        public bool IsDownloading { get; set; } = false;
        public DownloadState DownloadState { get; set; } = DownloadState.DOWNLOAD_ANY;
        public int DownloadProgress { get; set; } = 0;
    }
}
