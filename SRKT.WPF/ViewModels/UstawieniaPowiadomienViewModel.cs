using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.WPF.Services;
using System.Windows;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    public class UstawieniaPowiadomienViewModel : BaseViewModel
    {
        private readonly IPowiadomienieService _powiadomienieService;
        private readonly Uzytkownik _uzytkownik;

        // Kanały
        private bool _emailWlaczony = true;
        private bool _systemoweWlaczone = true;
        private bool _pushWlaczony = true;

        // Typy powiadomień
        private bool _powiadamiajONoweRezerwacje = true;
        private bool _powiadamiajOZmianyStatusu = true;
        private bool _powiadamiajPrzypomnienia = true;
        private bool _powiadamiajOPlatnosciach = true;
        private bool _powiadamiajOPromocjach = false;

        public UstawieniaPowiadomienViewModel(IPowiadomienieService powiadomienieService, Uzytkownik uzytkownik)
        {
            _powiadomienieService = powiadomienieService;
            _uzytkownik = uzytkownik;

            ZapiszCommand = new RelayCommand(_ => ZapiszUstawienia());
            TestujPowiadomienieCommand = new RelayCommand(async _ => await TestujPowiadomienieAsync());
        }

        #region Właściwości - Kanały

        public bool EmailWlaczony
        {
            get => _emailWlaczony;
            set => SetProperty(ref _emailWlaczony, value);
        }

        public bool SystemoweWlaczone
        {
            get => _systemoweWlaczone;
            set => SetProperty(ref _systemoweWlaczone, value);
        }

        public bool PushWlaczony
        {
            get => _pushWlaczony;
            set => SetProperty(ref _pushWlaczony, value);
        }

        #endregion

        #region Właściwości - Typy powiadomień

        public bool PowiadamiajONoweRezerwacje
        {
            get => _powiadamiajONoweRezerwacje;
            set => SetProperty(ref _powiadamiajONoweRezerwacje, value);
        }

        public bool PowiadamiajOZmianyStatusu
        {
            get => _powiadamiajOZmianyStatusu;
            set => SetProperty(ref _powiadamiajOZmianyStatusu, value);
        }

        public bool PowiadamiajPrzypomnienia
        {
            get => _powiadamiajPrzypomnienia;
            set => SetProperty(ref _powiadamiajPrzypomnienia, value);
        }

        public bool PowiadamiajOPlatnosciach
        {
            get => _powiadamiajOPlatnosciach;
            set => SetProperty(ref _powiadamiajOPlatnosciach, value);
        }

        public bool PowiadamiajOPromocjach
        {
            get => _powiadamiajOPromocjach;
            set => SetProperty(ref _powiadamiajOPromocjach, value);
        }

        #endregion

        #region Komendy

        public ICommand ZapiszCommand { get; }
        public ICommand TestujPowiadomienieCommand { get; }

        #endregion

        #region Metody

        private void ZapiszUstawienia()
        {
            // W pełnej implementacji zapisalibyśmy ustawienia do bazy/pliku
            // Na razie tylko komunikat

            MessageBox.Show(
                "Ustawienia powiadomień zostały zapisane!\n\n" +
                $"Email: {(EmailWlaczony ? "Włączony" : "Wyłączony")}\n" +
                $"Systemowe: {(SystemoweWlaczone ? "Włączone" : "Wyłączone")}\n" +
                $"Push/Toast: {(PushWlaczony ? "Włączony" : "Wyłączony")}",
                "Sukces",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async Task TestujPowiadomienieAsync()
        {
            // Testuj włączone kanały
            if (PushWlaczony)
            {
                // Toast - bezpośrednio przez ToastManager
                ToastManager.Instance.ShowInfo(
                    "Test powiadomienia",
                    "To jest testowe powiadomienie Pop-up (Toast)!");
            }

            if (SystemoweWlaczone && _powiadomienieService != null)
            {
                // Systemowe - przez serwis
                await _powiadomienieService.WyslijPowiadomienieAsync(
                    _uzytkownik.Id,
                    "Test powiadomienia systemowego",
                    "To jest testowe powiadomienie zapisane w aplikacji.",
                    TypPowiadomieniaEnum.Systemowe);
            }

            if (EmailWlaczony && _powiadomienieService != null)
            {
                // Email - symulacja
                await _powiadomienieService.WyslijPowiadomienieAsync(
                    _uzytkownik.Id,
                    "Test powiadomienia email",
                    "To jest testowe powiadomienie email (symulacja).",
                    TypPowiadomieniaEnum.Email);

                MessageBox.Show(
                    $"Email testowy został 'wysłany' na adres:\n{_uzytkownik.Email}\n\n" +
                    "(W tej wersji email jest symulowany - zapisywany do logu)",
                    "Email wysłany",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        #endregion
    }
}
