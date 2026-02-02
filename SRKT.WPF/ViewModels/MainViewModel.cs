using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using SRKT.WPF.Services;
using SRKT.WPF.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IKortRepository _kortRepo;
        private readonly IRezerwacjaService _rezerwacjaService;
        private readonly IRepository<Uzytkownik> _uzytkownikRepo;
        private readonly IPowiadomienieService _powiadomienieService;
        private readonly IPrzypomnienieService _przypomnienieService;

        private Uzytkownik _aktualnyUzytkownik;
        private object _aktualnyWidok;
        private int _liczbaNieprzeczytanych;

        public MainViewModel(
            IKortRepository kortRepo,
            IRezerwacjaService rezerwacjaService,
            IRepository<Uzytkownik> uzytkownikRepo,
            IPrzypomnienieService przypomnienieService = null,
            IPowiadomienieService powiadomienieService = null)
        {
            _kortRepo = kortRepo ?? throw new ArgumentNullException(nameof(kortRepo));
            _rezerwacjaService = rezerwacjaService ?? throw new ArgumentNullException(nameof(rezerwacjaService));
            _uzytkownikRepo = uzytkownikRepo ?? throw new ArgumentNullException(nameof(uzytkownikRepo));
            _przypomnienieService = przypomnienieService;
            _powiadomienieService = powiadomienieService;

            // Komendy nawigacji
            PokazDostepneKortyCommand = new RelayCommand(_ => PokazDostepneKorty());
            PokazMojeRezerwacjeCommand = new RelayCommand(_ => PokazMojeRezerwacje());
            PokazPowiadomieniaCommand = new RelayCommand(_ => PokazPowiadomienia());
            PokazPrzypomnieniCommand = new RelayCommand(_ => PokazPrzypomnienia());
            WylogujCommand = new RelayCommand(_ => Wyloguj());

            // Subskrybuj event Toast notifications
            if (_powiadomienieService != null)
            {
                _powiadomienieService.NowePowiadomienieToast += OnNowePowiadomienieToast;
            }

            // NIE pokazujemy domyślnego widoku tutaj - czekamy na ustawienie użytkownika
        }

        #region Właściwości

        public Uzytkownik AktualnyUzytkownik
        {
            get => _aktualnyUzytkownik;
            set
            {
                if (SetProperty(ref _aktualnyUzytkownik, value))
                {
                    // Załaduj liczbę nieprzeczytanych po ustawieniu użytkownika
                    if (value != null)
                    {
                        _ = ZaladujLiczbeNieprzeczytanychAsync();
                    }
                }
            }
        }

        public object AktualnyWidok
        {
            get => _aktualnyWidok;
            set => SetProperty(ref _aktualnyWidok, value);
        }

        public int LiczbaNieprzeczytanych
        {
            get => _liczbaNieprzeczytanych;
            set
            {
                if (SetProperty(ref _liczbaNieprzeczytanych, value))
                {
                    OnPropertyChanged(nameof(MaNieprzeczytanePowiadomienia));
                    OnPropertyChanged(nameof(WidocznoscBadgePowiadomien));
                }
            }
        }

        public bool MaNieprzeczytanePowiadomienia => LiczbaNieprzeczytanych > 0;

        public Visibility WidocznoscBadgePowiadomien =>
            MaNieprzeczytanePowiadomienia ? Visibility.Visible : Visibility.Collapsed;

        #endregion

        #region Komendy
        public ICommand PokazPrzypomnieniCommand { get; }
        public ICommand PokazDostepneKortyCommand { get; }
        public ICommand PokazMojeRezerwacjeCommand { get; }
        public ICommand PokazPowiadomieniaCommand { get; }
        public ICommand WylogujCommand { get; }

        #endregion

        #region Eventy

        public event EventHandler LogoutRequested;

        #endregion

        #region Metody nawigacji

        private void PokazDostepneKorty()
        {
            // Sprawdź czy użytkownik jest ustawiony
            if (AktualnyUzytkownik == null)
            {
                MessageBox.Show("Błąd: Nie zalogowano użytkownika.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var viewModel = new DostepneKortyViewModel(_kortRepo, _rezerwacjaService, AktualnyUzytkownik);
                var view = new DostepneKortyView { DataContext = viewModel };
                AktualnyWidok = view;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas ładowania widoku kortów: {ex.Message}",
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PokazMojeRezerwacje()
        {
            if (AktualnyUzytkownik == null)
            {
                MessageBox.Show("Błąd: Nie zalogowano użytkownika.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var viewModel = new MojeRezerwacjeViewModel(_rezerwacjaService, AktualnyUzytkownik);
                var view = new MojeRezerwacjeView { DataContext = viewModel };
                AktualnyWidok = view;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas ładowania rezerwacji: {ex.Message}",
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PokazPrzypomnienia()
        {
            if (AktualnyUzytkownik == null)
            {
                MessageBox.Show("Błąd: Nie zalogowano użytkownika.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                IPrzypomnienieService przypomnienieService = _przypomnienieService;

                if (przypomnienieService == null)
                {
                    // Spróbuj pobrać z DI
                    var app = Application.Current as App;
                    przypomnienieService = app?.ServiceProvider?.GetService(typeof(IPrzypomnienieService)) as IPrzypomnienieService;

                    if (przypomnienieService == null)
                    {
                        MessageBox.Show("Serwis przypomnień nie jest dostępny.\nSprawdź konfigurację aplikacji.",
                            "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                var viewModel = new PrzypomnieniViewModel(przypomnienieService, _rezerwacjaService, AktualnyUzytkownik);
                var view = new PrzypomnieniView { DataContext = viewModel };
                AktualnyWidok = view;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas ładowania przypomnień: {ex.Message}\n\nSzczegóły: {ex.StackTrace}",
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PokazPowiadomienia()
        {
            if (AktualnyUzytkownik == null)
            {
                MessageBox.Show("Błąd: Nie zalogowano użytkownika.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                IPowiadomienieService powiadomienieService = _powiadomienieService;

                if (powiadomienieService == null)
                {
                    // Spróbuj pobrać z DI
                    var app = Application.Current as App;
                    powiadomienieService = app?.ServiceProvider?.GetService(typeof(IPowiadomienieService)) as IPowiadomienieService;
                }

                if (powiadomienieService == null)
                {
                    MessageBox.Show("Serwis powiadomień nie jest dostępny.\nSprawdź konfigurację aplikacji.",
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var viewModel = new PowiadomieniaViewModel(powiadomienieService, AktualnyUzytkownik);
                var view = new PowiadomieniaView { DataContext = viewModel };
                AktualnyWidok = view;

                // Odśwież licznik po otwarciu widoku
                _ = ZaladujLiczbeNieprzeczytanychAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas ładowania powiadomień: {ex.Message}",
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Wyloguj()
        {
            // Odsubskrybuj event
            if (_powiadomienieService != null)
            {
                _powiadomienieService.NowePowiadomienieToast -= OnNowePowiadomienieToast;
            }

            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Obsługa powiadomień

        private async Task ZaladujLiczbeNieprzeczytanychAsync()
        {
            if (_powiadomienieService == null || AktualnyUzytkownik == null)
                return;

            try
            {
                LiczbaNieprzeczytanych = await _powiadomienieService.GetLiczbaNieprzeczytanychAsync(AktualnyUzytkownik.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd ładowania liczby powiadomień: {ex.Message}");
            }
        }

        private void OnNowePowiadomienieToast(object sender, PowiadomienieEventArgs e)
        {
            // Sprawdź czy powiadomienie jest dla aktualnego użytkownika
            if (AktualnyUzytkownik == null || e.UzytkownikId != AktualnyUzytkownik.Id)
                return;

            // Wyświetl Toast
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    ToastManager.Instance.ShowInfo(e.Tytul, e.Tresc);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Błąd wyświetlania toast: {ex.Message}");
                }
            });

            // Odśwież licznik
            _ = ZaladujLiczbeNieprzeczytanychAsync();
        }

        /// <summary>
        /// Metoda do ręcznego odświeżenia licznika powiadomień
        /// </summary>
        public async Task OdswiezPowiadomieniaAsync()
        {
            await ZaladujLiczbeNieprzeczytanychAsync();
        }

        #endregion
    }
}