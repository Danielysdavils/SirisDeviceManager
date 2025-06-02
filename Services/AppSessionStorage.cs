using SirisDeviceManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.Services
{
    public class AppSessionStorage
    {
        #region

        private AppSessionStorage(){}

        private static AppSessionStorage? instance;
        public static AppSessionStorage Instance
        {
            get
            {
                if(instance == null)
                    instance = new AppSessionStorage();

                return instance;
            }
        }

        #endregion


        public User User { get; set; } = new User();
        public Network Network { get; set; } = new();
        public List<Device> Devices { get; set; } = new();
        public Server Server { get; set; } = new();
    }
}
