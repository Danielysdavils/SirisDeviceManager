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

    public class Session
    {
        public bool IsRunning { get; set; } = false;
        public string SessionId { get; set; } = string.Empty;
        public SessionState SessionState { get; set; } = SessionState.SESSION_ANY;
    }
}
