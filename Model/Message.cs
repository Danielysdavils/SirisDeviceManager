using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.Model
{
    public enum MessateType
    {
        ERROR,
        SUCCESS,
        WARNING,
        INFO
    }

    public class Message
    {
        public string MessageText { get; set; } = string.Empty;
        public MessateType Type { get; set; } = MessateType.INFO; 
    }
}
