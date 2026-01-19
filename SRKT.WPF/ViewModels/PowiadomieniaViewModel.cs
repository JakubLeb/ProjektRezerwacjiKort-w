using SRKT.Business.Services;
using SRKT.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    public class PowiadomieniaViewModel : BaseViewModel
    {
        private readonly IPowiadomienieService _powiadomienieService;
        private readonly Uzytkownik _uzytkownik;

        private ObservableCollection<Powiadomienie> _powiadomienia;
        private ObservableCollection<Powiadomienie> _powiadomieniaFiltrowane;
        private int _liczbaNieprzeczytanych;

        // Filtry
        private bool _filtrWszystkie = true;
        private bool _filtrNieprzeczytane;
        private bool _filtrEmail;
        private bool _filtrSystemowe;

        public PowiadomieniaViewModel(IPowiadomienieService powiadomienieService, Uzytkownik uzytkownik)
        {
            _powiadomienieService = powiadomienieService;
            _uzytkownik = uzytkownik;

            Powiadomienia = new ObservableCollection<Powiadomienie>();
            PowiadomieniaFiltrowane = new ObservableCollection<Powiadomienie>();

            // Komendy
            OdswiezCommand = new RelayCommand(async _ => await ZaladujPowiadomieniaAsync());
            OznaczWszystkieCommand = new RelayCommand(async _ => await OznaczWszystkieJakoPrzeczytaneAsync());
            OznaczJakoPrzeczytaneCommand = new RelayCommand(async param => await OznaczJakoPrzeczytaneAsync(param as Powiadomienie));
            UsunPowiadomienieCommand = new RelayCommand(async param => await UsunPowiadomienieAsync(param as Powiadomienie));

            // Załaduj dane
            _ = ZaladujPowiadomieniaAsync();
        }

        #region Właściwości

        public ObservableCollection<Powiadomienie> Powiadomienia
        {
            get => _powiadomienia;
            set => SetProperty(ref _powiadomienia, value);
        }

        public ObservableCollection<Powiadomienie> PowiadomieniaFiltrowane
        {
            get => _powiadomieniaFiltrowane;
            set => SetProperty(ref _powiadomieniaFiltrowane, value);
        }

        public int LiczbaNieprzeczytanych
        {
            get => _liczbaNieprzeczytanych;
            set
            {
                if (SetProperty(ref _liczbaNieprzeczytanych, value))
                {
                    OnPropertyChanged(nameof(MaNieprzeczytane));
                }
            }
        }

        public bool MaNieprzeczytane => LiczbaNieprzeczytanych > 0;

        public bool BrakPowiadomien => PowiadomieniaFiltrowane?.Count == 0;

        // Filtry
        public bool FiltrWszystkie
        {
            get => _filtrWszystkie;
            set
            {
                if (SetProperty(ref _filtrWszystkie, value) && value)
                {
                    FiltrujPowiadomienia();
                }
            }
        }

        public bool FiltrNieprzeczytane
        {
            get => _filtrNieprzeczytane;
            set
            {
                if (SetProperty(ref _filtrNieprzeczytane, value) && value)
                {
                    FiltrujPowiadomienia();
                }
            }
        }

        public bool FiltrEmail
        {
            get => _filtrEmail;
            set
            {
                if (SetProperty(ref _filtrEmail, value) && value)
                {
                    FiltrujPowiadomienia();
                }
            }
        }

        public bool FiltrSystemowe
        {
            get => _filtrSystemowe;
            set
            {
                if (SetProperty(ref _filtrSystemowe, value) && value)
                {
                    FiltrujPowiadomienia();
                }
            }
        }

        #endregion

        #region Komendy

        public ICommand OdswiezCommand { get; }
        public ICommand OznaczWszystkieCommand { get; }
        public ICommand OznaczJakoPrzeczytaneCommand { get; }
        public ICommand UsunPowiadomienieCommand { get; }

        #endregion

        #region Metody

        private async Task ZaladujPowiadomieniaAsync()
        {
            try
            {
                var powiadomienia = await _powiadomienieService.GetPowiadomieniaUzytkownikaAsync(_uzytkownik.Id);

                Powiadomienia.Clear();
                foreach (var p in powiadomienia.OrderByDescending(x => x.DataUtworzenia))
                {
                    Powiadomienia.Add(p);
                }

                // Policz nieprzeczytane
                LiczbaNieprzeczytanych = await _powiadomienieService.GetLiczbaNieprzeczytanychAsync(_uzytkownik.Id);

                // Filtruj
                FiltrujPowiadomienia();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd ładowania powiadomień: {ex.Message}");
            }
        }

        private void FiltrujPowiadomienia()
        {
            IEnumerable<Powiadomienie> przefiltrowane = Powiadomienia;

            if (FiltrNieprzeczytane)
            {
                przefiltrowane = przefiltrowane.Where(p =>
                    p.StatusPowiadomieniaId == (int)StatusPowiadomieniaEnum.Wyslane ||
                    p.StatusPowiadomieniaId == (int)StatusPowiadomieniaEnum.Oczekujace);
            }
            else if (FiltrEmail)
            {
                przefiltrowane = przefiltrowane.Where(p => p.TypPowiadomieniaId == (int)TypPowiadomieniaEnum.Email);
            }
            else if (FiltrSystemowe)
            {
                przefiltrowane = przefiltrowane.Where(p => p.TypPowiadomieniaId == (int)TypPowiadomieniaEnum.Systemowe);
            }
            // FiltrWszystkie = brak filtrowania

            PowiadomieniaFiltrowane.Clear();
            foreach (var p in przefiltrowane)
            {
                PowiadomieniaFiltrowane.Add(p);
            }

            OnPropertyChanged(nameof(BrakPowiadomien));
        }

        private async Task OznaczWszystkieJakoPrzeczytaneAsync()
        {
            try
            {
                await _powiadomienieService.OznaczWszystkieJakoPrzeczytaneAsync(_uzytkownik.Id);
                await ZaladujPowiadomieniaAsync();

                MessageBox.Show("Wszystkie powiadomienia zostały oznaczone jako przeczytane.",
                    "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OznaczJakoPrzeczytaneAsync(Powiadomienie powiadomienie)
        {
            if (powiadomienie == null) return;

            // Sprawdź czy już przeczytane
            if (powiadomienie.StatusPowiadomieniaId == (int)StatusPowiadomieniaEnum.Przeczytane)
                return;

            try
            {
                await _powiadomienieService.OznaczJakoPrzeczytaneAsync(powiadomienie.Id);
                await ZaladujPowiadomieniaAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd oznaczania: {ex.Message}");
            }
        }

        private async Task UsunPowiadomienieAsync(Powiadomienie powiadomienie)
        {
            if (powiadomienie == null) return;

            var result = MessageBox.Show(
                "Czy na pewno chcesz usunąć to powiadomienie?",
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _powiadomienieService.UsunPowiadomienieAsync(powiadomienie.Id);
                    await ZaladujPowiadomieniaAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd usuwania: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion
    }
}
