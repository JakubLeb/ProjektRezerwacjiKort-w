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
        private bool _isLoading;

        // Filtry
        private bool _filtrWszystkie = true;
        private bool _filtrNieprzeczytane;
        private bool _filtrEmail;
        private bool _filtrSystemowe;

        public PowiadomieniaViewModel(IPowiadomienieService powiadomienieService, Uzytkownik uzytkownik)
        {
            _powiadomienieService = powiadomienieService ?? throw new ArgumentNullException(nameof(powiadomienieService));
            _uzytkownik = uzytkownik ?? throw new ArgumentNullException(nameof(uzytkownik));

            Powiadomienia = new ObservableCollection<Powiadomienie>();
            PowiadomieniaFiltrowane = new ObservableCollection<Powiadomienie>();

            // Komendy
            OdswiezCommand = new RelayCommand(async _ => await ZaladujPowiadomieniaAsync(), _ => !IsLoading);
            OznaczWszystkieCommand = new RelayCommand(async _ => await OznaczWszystkieJakoPrzeczytaneAsync(), _ => !IsLoading);
            OznaczJakoPrzeczytaneCommand = new RelayCommand(async param => await OznaczJakoPrzeczytaneAsync(param as Powiadomienie), _ => !IsLoading);
            UsunPowiadomienieCommand = new RelayCommand(async param => await UsunPowiadomienieAsync(param as Powiadomienie), _ => !IsLoading);

            // Załaduj dane
            _ = ZaladujPowiadomieniaAsync();
        }

        #region Właściwości

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

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
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                var powiadomienia = await _powiadomienieService.GetPowiadomieniaUzytkownikaAsync(_uzytkownik.Id);

                // Aktualizuj na wątku UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Powiadomienia.Clear();
                    if (powiadomienia != null)
                    {
                        foreach (var p in powiadomienia.OrderByDescending(x => x.DataUtworzenia))
                        {
                            Powiadomienia.Add(p);
                        }
                    }

                    // Policz nieprzeczytane
                    LiczbaNieprzeczytanych = Powiadomienia.Count(p =>
                        p.StatusPowiadomieniaId == (int)StatusPowiadomieniaEnum.Wyslane ||
                        p.StatusPowiadomieniaId == (int)StatusPowiadomieniaEnum.Oczekujace);

                    // Filtruj
                    FiltrujPowiadomienia();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd ładowania powiadomień: {ex.Message}");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Błąd ładowania powiadomień: {ex.Message}",
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FiltrujPowiadomienia()
        {
            try
            {
                IEnumerable<Powiadomienie> przefiltrowane = Powiadomienia ?? Enumerable.Empty<Powiadomienie>();

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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd filtrowania: {ex.Message}");
            }
        }

        private async Task OznaczWszystkieJakoPrzeczytaneAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                await _powiadomienieService.OznaczWszystkieJakoPrzeczytaneAsync(_uzytkownik.Id);

                // Odśwież listę
                await ZaladujPowiadomieniaAsync();

                MessageBox.Show("Wszystkie powiadomienia zostały oznaczone jako przeczytane.",
                    "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OznaczJakoPrzeczytaneAsync(Powiadomienie powiadomienie)
        {
            if (powiadomienie == null || IsLoading) return;

            // Sprawdź czy już przeczytane
            if (powiadomienie.StatusPowiadomieniaId == (int)StatusPowiadomieniaEnum.Przeczytane)
                return;

            try
            {
                IsLoading = true;

                await _powiadomienieService.OznaczJakoPrzeczytaneAsync(powiadomienie.Id);

                // Odśwież listę
                await ZaladujPowiadomieniaAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd oznaczania: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UsunPowiadomienieAsync(Powiadomienie powiadomienie)
        {
            if (powiadomienie == null || IsLoading) return;

            var result = MessageBox.Show(
                "Czy na pewno chcesz usunąć to powiadomienie?",
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;

                    await _powiadomienieService.UsunPowiadomienieAsync(powiadomienie.Id);

                    // Odśwież listę
                    await ZaladujPowiadomieniaAsync();

                    MessageBox.Show("Powiadomienie zostało usunięte.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd usuwania: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        #endregion
    }
}