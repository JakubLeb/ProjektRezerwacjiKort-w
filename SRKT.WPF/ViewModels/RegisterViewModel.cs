using SRKT.Business.Services;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        private string _email;
        private string _imie;
        private string _nazwisko;
        private string _haslo;
        private string _potwierdzHaslo;
        private string _komunikat;

        public RegisterViewModel(IAuthService authService)
        {
            _authService = authService;
            ZarejestrujCommand = new RelayCommand(async _ => await ZarejestrujAsync(), _ => MozeZarejestrowac());
            PowrotCommand = new RelayCommand(_ => ZamknijOkno());
        }

        // Właściwości
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Imie { get => _imie; set => SetProperty(ref _imie, value); }
        public string Nazwisko { get => _nazwisko; set => SetProperty(ref _nazwisko, value); }
        public string Haslo { get => _haslo; set => SetProperty(ref _haslo, value); }
        public string PotwierdzHaslo { get => _potwierdzHaslo; set => SetProperty(ref _potwierdzHaslo, value); }
        public string Komunikat { get => _komunikat; set => SetProperty(ref _komunikat, value); }

        public ICommand ZarejestrujCommand { get; }
        public ICommand PowrotCommand { get; }

        public event EventHandler RegistrationSuccessful;
        public event EventHandler NavigateBack;

        private bool CzyEmailJestPoprawny(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Wzorzec wymagający znaków przed @, znaku @, znaków po @, kropki i znaków po kropce
            string wzorzec = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            return Regex.IsMatch(email, wzorzec);
        }
        private bool MozeZarejestrowac()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                    CzyEmailJestPoprawny(Email) &&
                   !string.IsNullOrWhiteSpace(Haslo) &&
                   !string.IsNullOrWhiteSpace(Imie) &&
                   !string.IsNullOrWhiteSpace(Nazwisko) &&
                   Haslo == PotwierdzHaslo;
        }

        private async Task ZarejestrujAsync()
        {
            try
            {
                if (!CzyEmailJestPoprawny(Email))
                {
                    Komunikat = "Podany adres email ma nieprawidłowy format.";
                    return;
                }

                bool sukces = await _authService.RegisterAsync(Email, Haslo, Imie, Nazwisko);
                if (sukces)
                {
                    MessageBox.Show("Rejestracja przebiegła pomyślnie! Możesz się teraz zalogować.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    RegistrationSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Komunikat = "Użytkownik o podanym adresie email już istnieje.";
                }
            }
            catch (Exception ex)
            {
                Komunikat = $"Błąd: {ex.Message}";
            }
        }

        private void ZamknijOkno()
        {
            NavigateBack?.Invoke(this, EventArgs.Empty);
        }
    }
}