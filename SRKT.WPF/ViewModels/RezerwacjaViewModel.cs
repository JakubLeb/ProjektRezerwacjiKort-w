using SRKT.Business.Services;
using SRKT.Core.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    public class RezerwacjaViewModel : BaseViewModel
    {
        private readonly OpcjaRezerwacji _opcja;
        private readonly IRezerwacjaService _rezerwacjaService;
        private readonly Uzytkownik _uzytkownik;
        private readonly Action _closeAction;

        private bool _czyPlatnoscNaMiejscu = true;
        private bool _czyPlatnoscBlik;
        private string _kodBlik;
        private string _blikError;

        public RezerwacjaViewModel(
            OpcjaRezerwacji opcja,
            IRezerwacjaService rezerwacjaService,
            Uzytkownik uzytkownik,
            Action closeAction)
        {
            _opcja = opcja;
            _rezerwacjaService = rezerwacjaService;
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
                    BlikError = string.Empty; // Reset błędu przy zmianie
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

        public string KodBlik
        {
            get => _kodBlik;
            set
            {
                if (SetProperty(ref _kodBlik, value))
                {
                    BlikError = string.Empty; // Reset błędu przy wpisywaniu
                }
            }
        }

        public string BlikError { get => _blikError; set => SetProperty(ref _blikError, value); }
        public bool MaBlikError => !string.IsNullOrEmpty(BlikError);

        // --- Komendy ---
        public ICommand PotwierdzCommand { get; }
        public ICommand AnulujCommand { get; }

        private async Task PotwierdzAsync()
        {
            // Walidacja BLIK
            if (CzyPlatnoscBlik)
            {
                if (string.IsNullOrWhiteSpace(KodBlik) || KodBlik.Length != 6 || !KodBlik.All(char.IsDigit))
                {
                    BlikError = "Kod BLIK musi składać się z 6 cyfr.";
                    return;
                }
                // Tutaj normalnie nastąpiłaby komunikacja z bramką płatności
            }

            try
            {
                // Wywołanie serwisu
                await _rezerwacjaService.UtworzRezerwacjeAsync(
                    _opcja.Slot.KortId,
                    _uzytkownik.Id,
                    _opcja.Slot.Start,
                    _opcja.Slot.Dlugosc,
                    null // Tutaj można przekazać ID płatności jeśli system to obsługuje
                );

                MessageBox.Show("Rezerwacja zakończona sukcesem!", "Sukces");
                _closeAction(); // Zamknij okno
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd rezerwacji: {ex.Message}", "Błąd");
            }
        }
    }
}