using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    public class AdminKalendarzViewModel : BaseViewModel
    {
        private readonly IRezerwacjaRepository _rezerwacjaRepo;
        private readonly IKortRepository _kortRepo;
        private readonly IRepository<ObiektSportowy> _obiektRepo;

        private ObservableCollection<ObiektSportowy> _obiekty;
        private ObservableCollection<Kort> _korty;
        private ObservableCollection<RezerwacjaKalendarza> _rezerwacjeKalendarza;
        private ObiektSportowy _wybranyObiekt;
        private Kort _wybranyKort;
        private DateTime _wybranaData = DateTime.Today;
        private RezerwacjaKalendarza _wybranaRezerwacja;

        public AdminKalendarzViewModel(
            IRezerwacjaRepository rezerwacjaRepo,
            IKortRepository kortRepo,
            IRepository<ObiektSportowy> obiektRepo)
        {
            _rezerwacjaRepo = rezerwacjaRepo;
            _kortRepo = kortRepo;
            _obiektRepo = obiektRepo;

            Obiekty = new ObservableCollection<ObiektSportowy>();
            Korty = new ObservableCollection<Kort>();
            RezerwacjeKalendarza = new ObservableCollection<RezerwacjaKalendarza>();

            ZaladujDaneCommand = new RelayCommand(async _ => await ZaladujDaneAsync());
            OdswiezKalendarzCommand = new RelayCommand(async _ => await OdswiezKalendarzAsync());
            AnulujRezerwacjeCommand = new RelayCommand(async _ => await AnulujRezerwacjeAsync(), _ => WybranaRezerwacja != null);
            PoprzedniDzienCommand = new RelayCommand(_ => { WybranaData = WybranaData.AddDays(-1); _ = OdswiezKalendarzAsync(); });
            NastepnyDzienCommand = new RelayCommand(_ => { WybranaData = WybranaData.AddDays(1); _ = OdswiezKalendarzAsync(); });
            DzisCommand = new RelayCommand(_ => { WybranaData = DateTime.Today; _ = OdswiezKalendarzAsync(); });

            _ = ZaladujDaneAsync();
        }

        // Właściwości
        public ObservableCollection<ObiektSportowy> Obiekty
        {
            get => _obiekty;
            set => SetProperty(ref _obiekty, value);
        }

        public ObservableCollection<Kort> Korty
        {
            get => _korty;
            set => SetProperty(ref _korty, value);
        }

        public ObservableCollection<RezerwacjaKalendarza> RezerwacjeKalendarza
        {
            get => _rezerwacjeKalendarza;
            set => SetProperty(ref _rezerwacjeKalendarza, value);
        }

        public ObiektSportowy WybranyObiekt
        {
            get => _wybranyObiekt;
            set
            {
                if (SetProperty(ref _wybranyObiekt, value))
                {
                    _ = ZaladujKortyObiektuAsync();
                }
            }
        }

        public Kort WybranyKort
        {
            get => _wybranyKort;
            set
            {
                if (SetProperty(ref _wybranyKort, value))
                {
                    _ = OdswiezKalendarzAsync();
                }
            }
        }

        public DateTime WybranaData
        {
            get => _wybranaData;
            set => SetProperty(ref _wybranaData, value);
        }

        public RezerwacjaKalendarza WybranaRezerwacja
        {
            get => _wybranaRezerwacja;
            set => SetProperty(ref _wybranaRezerwacja, value);
        }

        // Komendy
        public ICommand ZaladujDaneCommand { get; }
        public ICommand OdswiezKalendarzCommand { get; }
        public ICommand AnulujRezerwacjeCommand { get; }
        public ICommand PoprzedniDzienCommand { get; }
        public ICommand NastepnyDzienCommand { get; }
        public ICommand DzisCommand { get; }

        // Metody
        private async Task ZaladujDaneAsync()
        {
            try
            {
                var obiekty = await _obiektRepo.GetAllAsync();
                Obiekty.Clear();
                foreach (var obiekt in obiekty.Where(o => o.CzyAktywny))
                {
                    Obiekty.Add(obiekt);
                }

                if (Obiekty.Any())
                {
                    WybranyObiekt = Obiekty.First();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd ładowania danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ZaladujKortyObiektuAsync()
        {
            if (WybranyObiekt == null) return;

            try
            {
                var korty = await _kortRepo.GetKortyByObiektAsync(WybranyObiekt.Id);
                Korty.Clear();

                // Dodaj opcję "Wszystkie korty"
                Korty.Add(new Kort { Id = 0, Nazwa = "Wszystkie korty", ObiektSportowyId = WybranyObiekt.Id });

                foreach (var kort in korty.Where(k => k.CzyAktywny))
                {
                    Korty.Add(kort);
                }

                WybranyKort = Korty.First();
                await OdswiezKalendarzAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd ładowania kortów: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OdswiezKalendarzAsync()
        {
            if (WybranyObiekt == null) return;

            try
            {
                RezerwacjeKalendarza.Clear();

                // Pobierz rezerwacje dla całego dnia
                var dataStart = WybranaData.Date;
                var dataEnd = WybranaData.Date.AddDays(1);

                IEnumerable<Rezerwacja> rezerwacje;

                if (WybranyKort != null && WybranyKort.Id > 0)
                {
                    // Rezerwacje dla wybranego kortu
                    rezerwacje = await _rezerwacjaRepo.GetRezerwacjeByKortAsync(WybranyKort.Id, WybranaData);
                }
                else
                {
                    // Rezerwacje dla wszystkich kortów obiektu
                    var wszystkieRezerwacje = await _rezerwacjaRepo.GetRezerwacjeByDataAsync(dataStart, dataEnd);
                    rezerwacje = wszystkieRezerwacje.Where(r => r.Kort?.ObiektSportowyId == WybranyObiekt.Id);
                }

                // Generuj widok kalendarza (8:00 - 22:00)
                for (int godzina = 8; godzina < 22; godzina++)
                {
                    var czasSlotu = new DateTime(WybranaData.Year, WybranaData.Month, WybranaData.Day, godzina, 0, 0);

                    if (WybranyKort != null && WybranyKort.Id > 0)
                    {
                        // Widok dla pojedynczego kortu
                        var rezerwacjaWSlot = rezerwacje.FirstOrDefault(r =>
                            r.DataRezerwacji <= czasSlotu &&
                            r.DataRezerwacji.AddHours((double)r.IloscGodzin) > czasSlotu &&
                            r.StatusRezerwacjiId != 3);

                        RezerwacjeKalendarza.Add(new RezerwacjaKalendarza
                        {
                            Godzina = $"{godzina:00}:00",
                            NazwaKortu = WybranyKort.Nazwa,
                            Rezerwacja = rezerwacjaWSlot,
                            Status = rezerwacjaWSlot != null ? "Zajęty" : "Wolny",
                            Klient = rezerwacjaWSlot?.Uzytkownik?.PelneImieNazwisko ?? "-",
                            CzyZajety = rezerwacjaWSlot != null,
                            CzyOplacona = rezerwacjaWSlot?.CzyOplacona ?? false
                        });
                    }
                    else
                    {
                        // Widok dla wszystkich kortów - pokaż każdy kort osobno
                        foreach (var kort in Korty.Where(k => k.Id > 0))
                        {
                            var rezerwacjaWSlot = rezerwacje.FirstOrDefault(r =>
                                r.KortId == kort.Id &&
                                r.DataRezerwacji <= czasSlotu &&
                                r.DataRezerwacji.AddHours((double)r.IloscGodzin) > czasSlotu &&
                                r.StatusRezerwacjiId != 3);

                            RezerwacjeKalendarza.Add(new RezerwacjaKalendarza
                            {
                                Godzina = $"{godzina:00}:00",
                                NazwaKortu = kort.Nazwa,
                                Rezerwacja = rezerwacjaWSlot,
                                Status = rezerwacjaWSlot != null ? "Zajęty" : "Wolny",
                                Klient = rezerwacjaWSlot?.Uzytkownik?.PelneImieNazwisko ?? "-",
                                CzyZajety = rezerwacjaWSlot != null,
                                CzyOplacona = rezerwacjaWSlot?.CzyOplacona ?? false
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd odświeżania kalendarza: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AnulujRezerwacjeAsync()
        {
            if (WybranaRezerwacja?.Rezerwacja == null) return;

            var result = MessageBox.Show(
                $"Czy na pewno chcesz anulować rezerwację?\n\nKlient: {WybranaRezerwacja.Klient}\nGodzina: {WybranaRezerwacja.Godzina}",
                "Potwierdzenie anulowania",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var rezerwacja = WybranaRezerwacja.Rezerwacja;
                    rezerwacja.StatusRezerwacjiId = 3; // Anulowana
                    rezerwacja.DataModyfikacji = DateTime.Now;

                    await _rezerwacjaRepo.UpdateAsync(rezerwacja);

                    MessageBox.Show("Rezerwacja została anulowana.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    await OdswiezKalendarzAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd anulowania rezerwacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    // Klasa pomocnicza dla widoku kalendarza
    public class RezerwacjaKalendarza
    {
        public string Godzina { get; set; }
        public string NazwaKortu { get; set; }
        public Rezerwacja Rezerwacja { get; set; }
        public string Status { get; set; }
        public string Klient { get; set; }
        public bool CzyZajety { get; set; }
        public bool CzyOplacona { get; set; }
    }
}