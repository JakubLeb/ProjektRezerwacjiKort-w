using SRKT.Business.Services;
using SRKT.Core.Models;
using System.Windows;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private string _email;
        private string _haslo;
        private string _komunikat;
        private bool _isLoggingIn = false;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            ZalogujCommand = new RelayCommand(async _ => await ZalogujAsync(), _ => MozeZalogowac() && !_isLoggingIn);
            ZarejestrujCommand = new RelayCommand(_ => PrzejdzDoRejestracji());
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Haslo
        {
            get => _haslo;
            set => SetProperty(ref _haslo, value);
        }

        public string Komunikat
        {
            get => _komunikat;
            set => SetProperty(ref _komunikat, value);
        }

        public ICommand ZalogujCommand { get; }
        public ICommand ZarejestrujCommand { get; }

        public event EventHandler<Uzytkownik> LoginSuccessful;
        public event EventHandler NavigateToRegister;

        private bool MozeZalogowac()
        {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Haslo);
        }

        private async Task ZalogujAsync()
        {
            if (_isLoggingIn) return;

            _isLoggingIn = true;
            Komunikat = string.Empty;

            try
            {
                var uzytkownik = await _authService.LoginAsync(Email, Haslo);
                if (uzytkownik != null)
                {
                    LoginSuccessful?.Invoke(this, uzytkownik);
                }
                else
                {
                    Komunikat = "Nieprawidłowy email lub hasło.";
                }
            }
            catch (Exception ex)
            {
                Komunikat = $"Błąd: {ex.Message}";
            }
            finally
            {
                _isLoggingIn = false;
            }
        }

        private void PrzejdzDoRejestracji()
        {
            NavigateToRegister?.Invoke(this, EventArgs.Empty);
        }
    }
}