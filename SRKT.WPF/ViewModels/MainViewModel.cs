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
            _kortRepo = kortRepo;
            _rezerwacjaService = rezerwacjaService;
            _uzytkownikRepo = uzytkownikRepo;
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
                    _ = ZaladujLiczbeNieprzeczytanychAsync();
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
            var viewModel = new DostepneKortyViewModel(_kortRepo, _rezerwacjaService, AktualnyUzytkownik);
            var view = new DostepneKortyView { DataContext = viewModel };
            AktualnyWidok = view;
        }

        private void PokazMojeRezerwacje()
        {
            var viewModel = new MojeRezerwacjeViewModel(_rezerwacjaService, AktualnyUzytkownik);
            var view = new MojeRezerwacjeView { DataContext = viewModel };
            AktualnyWidok = view;
        }

        private void PokazPrzypomnienia()
        {
            if (_przypomnienieService == null || _rezerwacjaService == null)
            {
                MessageBox.Show("Serwis przypomnień nie jest dostępny.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var viewModel = new PrzypomnieniViewModel(_przypomnienieService, _rezerwacjaService, AktualnyUzytkownik);
            var view = new PrzypomnieniView { DataContext = viewModel };
            AktualnyWidok = view;
        }

        private void PokazPowiadomienia()
        {
            if (_powiadomienieService == null)
            {
                MessageBox.Show("Serwis powiadomień nie jest dostępny.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var viewModel = new PowiadomieniaViewModel(_powiadomienieService, AktualnyUzytkownik);
            var view = new PowiadomieniaView { DataContext = viewModel };
            AktualnyWidok = view;

            // Odśwież licznik po otwarciu widoku
            _ = ZaladujLiczbeNieprzeczytanychAsync();
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
                ToastManager.Instance.ShowInfo(e.Tytul, e.Tresc);
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
