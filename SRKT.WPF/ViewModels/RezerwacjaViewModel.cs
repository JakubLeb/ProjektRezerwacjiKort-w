using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.WPF.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using SRKT.Business.Services;
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
            _opcja = opcja;
            _rezerwacjaService = rezerwacjaService;
            _platnoscService = platnoscService;
            _uzytkownik = uzytkownik;
            _closeAction = closeAction;

            // Obliczenie ceny: Cena za godzinę * czas trwania
            CenaCalkowita = opcja.CenaZaGodzine * opcja.Slot.Dlugosc;

            PotwierdzCommand = new RelayCommand(async _ => await PotwierdzAsync());
            AnulujCommand = new RelayCommand(_ => _closeAction());
        }

        // --- Dane do wyświetlenia ---
        public string NazwaObiektu => _opcja.NazwaObiektu;
        public string AdresObiektu => _opcja.AdresObiektu;
        public DateTime DataRezerwacji => _opcja.Slot.Start.Date;
        public string ZakresGodzin => $"{_opcja.GodzinaStart} - {_opcja.GodzinaKoniec}";
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
                // Utwórz rezerwację
                var nowaRezerwacja = await _rezerwacjaService.UtworzRezerwacjeAsync(
                    _opcja.Slot.KortId,
                    _uzytkownik.Id,
                    _opcja.Slot.Start,
                    _opcja.Slot.Dlugosc,
                    Uwagi
                );

                // Jeśli wybrano płatność BLIK - otwórz okno płatności
                if (CzyPlatnoscBlik)
                {
                    var blikWindow = new BlikPaymentWindow();
                    blikWindow.Owner = Application.Current.MainWindow;

                    var blikViewModel = new BlikPaymentViewModel(
                        _platnoscService,
                        nowaRezerwacja.Id,
                        CenaCalkowita,
                        (sukces) =>
                        {
                            blikWindow.Close();

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
                    // Płatność na miejscu
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
                MessageBox.Show($"Błąd rezerwacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}