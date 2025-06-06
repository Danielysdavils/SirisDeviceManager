using SirisDeviceManager.ViewModel;
using SirisDeviceManager.ViewModel.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SirisDeviceManager.View.UserControls.Login
{
    /// <summary>
    /// Interação lógica para Login.xam
    /// </summary>
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SirisDeviceManagerViewModel mainVm &&
                mainVm.LoginViewModel != null)
            {
                mainVm.LoginViewModel.Password = PasswordBox.Password;
            }
        }
    }
}
