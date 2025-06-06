using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirisDeviceManager.Services
{
    public class PageService
    {
        #region

        private PageService() { }

        public static PageService? instance = null;
        public static PageService Instance
        {
            get
            {
                instance ??= new();
                return instance;
            }
        }

        #endregion

        private int appIndex = 0;
        public int AppIndex
        {
            get
            {
                return appIndex;
            }
            set
            {
                appIndex = value;
                ChangeManagmentIndexEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        private int managerIndex = 0;
        public int ManagerIndex
        {
            get
            {
                return managerIndex;

            }

            set
            {
                managerIndex = value;
                ChangeManagmentIndexEvent?.Invoke(this, EventArgs.Empty);   
            }
        }

        public EventHandler? ChangeAppIndexEvent;
        public EventHandler? ChangeManagmentIndexEvent;
    }
}
