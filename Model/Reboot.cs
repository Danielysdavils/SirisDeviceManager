using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.Model
{
    public class Reboot
    {
        public bool IsRebootScheduled { get; set; } = false;
        public TimeSpan RebootCountDown { get; set; } = TimeSpan.Zero;
    }
}
