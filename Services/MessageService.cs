using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.Services
{
    public sealed class MessageService
    {
        private static readonly Lazy<MessageService> _instance = new Lazy<MessageService>(() => new MessageService());

        public static MessageService Instance => _instance.Value;
        public event Action<string> StatusChanged;
        
        private MessageService() { }

        public void UpdateStatus(string message)
        {
            StatusChanged?.Invoke(message);
        }
    }
}
