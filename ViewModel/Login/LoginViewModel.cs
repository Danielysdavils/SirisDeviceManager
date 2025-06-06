using SiRISApi.Services;
using SirisDeviceManager.Base;
using SirisDeviceManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SiRISApi.Repository;
using Microsoft.EntityFrameworkCore;
using static System.Windows.Forms.AxHost;


namespace SirisDeviceManager.ViewModel.Login
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string _state = string.Empty;
        public string State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        public SirisDeviceManagerViewModel SirisDeviceManagerViewModel { get; set; }
        public event EventHandler? Authenticated;
        public ICommand LoginCommand { get; }
        
        public LoginViewModel(SirisDeviceManagerViewModel vm) 
        {
            SirisDeviceManagerViewModel = vm;

            LoginCommand = new RelayCommand<object>(_ => LoginCallback());
        }

        public void LoginCallback()
        {
            Thread t = new Thread(ExecuteLoginCommand);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private async void ExecuteLoginCommand()
        {
            try
            {
               State = "Logando...";
               Context context = new Context();
                var user = await context.Users
                    .Where(u => u.Login == Username && u.Password == Password)
                    .FirstOrDefaultAsync();

                if(user != null)
                {
                    SirisDeviceManager.Model.User u = new()
                    {
                        Username = user.Login,
                        Password = user.Password,
                    };
                    
                    AppSessionStorage.Instance.User = u;

                    State = "Login feito com sucesso! Aguarde...";
                    Authenticated?.Invoke(this, new EventArgs());
                }
                else
                    State = "Login inválido. User não encontrado";

            }catch(Exception ex)
            {
                State = $"[ERROR]: {ex.Message}";
            }
        }
    }
}
