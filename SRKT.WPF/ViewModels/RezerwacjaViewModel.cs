// SRKT.WPF/ViewModels/RezerwacjaViewModel.cs

using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.WPF.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    public class RezerwacjaViewModel : BaseViewModel
    {
        private readonly OpcjaRezerwacji _opcja;
        private readonly IRezerwacjaService _rezerwacjaService;
        private readonly IPlatnoscService _platnoscService;
        private readonly Uzytkownik _uzytkownik;
        private readonly Action _closeAction;

        private bool _czyPlatnoscNaMiejscu = true;
        private bool _czyPlatnoscBlik;
        private string _uwagi;

        public RezerwacjaViewModel(
            OpcjaRezerwacji opcja,
            IRezerwacjaService rezerwacjaService,
            IPlatnoscService platnoscService,
            Uzytkownik uzytkownik,
            Action closeAction)
        {
            _opcja = opcja ?? throw new ArgumentNullException(nameof(opcja));
            _rezerwacjaService = rezerwacjaService ?? throw new ArgumentNullException(nameof(rezerwacjaService));
            _platnoscService = platnoscService; // Może być null
            _uzytkownik = uzytkownik ?? throw new ArgumentNullException(nameof(uzytkownik));
            _closeAction = closeAction ?? throw new ArgumentNullException(nameof(closeAction));

            // Bezpieczne obliczenie ceny
            if (_opcja.Slot != null)
            {
                CenaCalkowita = _opcja.CenaZaGodzine * _opcja.Slot.Dlugosc;
            }
            else
            {
                CenaCalkowita = 0;
                MessageBox.Show("Ostrzeżenie: Brak danych o terminie.", "Ostrzeżenie", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            PotwierdzCommand = new RelayCommand(async _ => await PotwierdzAsync());
            AnulujCommand = new RelayCommand(_ => _closeAction());
        }

        // --- Dane do wyświetlenia ---
        public string NazwaObiektu => _opcja?.NazwaObiektu ?? "Nieznany obiekt";
        public string AdresObiektu => _opcja?.AdresObiektu ?? "Brak adresu";
        public DateTime DataRezerwacji => _opcja?.Slot?.Start.Date ?? DateTime.Today;
        public string ZakresGodzin => _opcja?.Slot != null
            ? $"{_opcja.GodzinaStart} - {_opcja.GodzinaKoniec}"
            : "Brak danych";
        public decimal CenaCalkowita { get; }

        // --- Obsługa Płatności ---
        public bool CzyPlatnoscNaMiejscu
        {
            get => _czyPlatnoscNaMiejscu;
            set
            {
                if (SetProperty(ref _czyPlatnoscNaMiejscu, value))
                {
                    if (value) CzyPlatnoscBlik = false;
                }
            }
        }

        public bool CzyPlatnoscBlik
        {
            get => _czyPlatnoscBlik;
            set
            {
                if (SetProperty(ref _czyPlatnoscBlik, value))
                {
                    if (value) CzyPlatnoscNaMiejscu = false;
                }
            }
        }

        public string Uwagi
        {
            get => _uwagi;
            set => SetProperty(ref _uwagi, value);
        }

        // --- Komendy ---
        public ICommand PotwierdzCommand { get; }
        public ICommand AnulujCommand { get; }

        private async Task PotwierdzAsync()
        {
            try
            {
                // Walidacja...
                if (_opcja?.Slot == null)
                {
                    MessageBox.Show("Błąd: Brak danych o wybranym terminie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_opcja.Slot.KortId <= 0)
                {
                    MessageBox.Show("Błąd: Nieprawidłowy identyfikator kortu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Jeśli płatność BLIK - utwórz rezerwację BEZ powiadomienia
                if (CzyPlatnoscBlik)
                {
                    var nowaRezerwacja = await _rezerwacjaService.UtworzRezerwacjeBezPowiadomieniaAsync(
                        _opcja.Slot.KortId,
                        _uzytkownik.Id,
                        _opcja.Slot.Start,
                        _opcja.Slot.Dlugosc,
                        Uwagi
                    );

                    if (nowaRezerwacja == null)
                    {
                        MessageBox.Show("Nie udało się utworzyć rezerwacji.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (_platnoscService == null)
                    {
                        // Wyślij powiadomienie (płatność niedostępna)
                        await _rezerwacjaService.WyslijPowiadomienieORezerwacjiAsync(nowaRezerwacja.Id, false);

                        MessageBox.Show(
                            "Rezerwacja została utworzona.\nPłatność BLIK jest chwilowo niedostępna - zapłać na miejscu.",
                            "Rezerwacja utworzona",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        _closeAction();
                        return;
                    }

                    var blikWindow = new BlikPaymentWindow();
                    blikWindow.Owner = Application.Current.MainWindow;

                    var blikViewModel = new BlikPaymentViewModel(
                        _platnoscService,
                        nowaRezerwacja.Id,
                        CenaCalkowita,
                        async (sukces) =>
                        {
                            blikWindow.Close();

                            // Wyślij powiadomienie DOPIERO TERAZ
                            await _rezerwacjaService.WyslijPowiadomienieORezerwacjiAsync(nowaRezerwacja.Id, sukces);

                            if (sukces)
                            {
                                MessageBox.Show(
                                    "Rezerwacja została utworzona i opłacona!",
                                    "Sukces",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Rezerwacja została utworzona.\nPłatność można dokonać na miejscu.",
                                    "Rezerwacja utworzona",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                            }

                            _closeAction();
                        }
                    );

                    blikWindow.DataContext = blikViewModel;
                    blikWindow.ShowDialog();
                }
                else
                {
                    // Płatność na miejscu - standardowe zachowanie z powiadomieniem
                    var nowaRezerwacja = await _rezerwacjaService.UtworzRezerwacjeAsync(
                        _opcja.Slot.KortId,
                        _uzytkownik.Id,
                        _opcja.Slot.Start,
                        _opcja.Slot.Dlugosc,
                        Uwagi
                    );

                    if (nowaRezerwacja == null)
                    {
                        MessageBox.Show("Nie udało się utworzyć rezerwacji.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    MessageBox.Show(
                        "Rezerwacja zakończona sukcesem!\nPłatność do uregulowania na miejscu.",
                        "Sukces",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    _closeAction();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}