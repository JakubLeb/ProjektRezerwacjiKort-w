using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using SRKT.WPF.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    // Wrapper (Opakowanie) dla wyniku wyszukiwania
    public class OpcjaRezerwacji
    {
        public TimeSlot Slot { get; set; }
        public string AdresObiektu { get; set; }
        public string NazwaObiektu { get; set; }
        public string LinkDoMapy { get; set; }
        public string ZdjecieSciezka { get; set; }

        // NOWE POLE:
        public decimal CenaZaGodzine { get; set; }

        // Właściwości pomocnicze dla XAML - z obsługą null
        public string GodzinaStart => Slot?.Start.ToString("HH:mm") ?? "--:--";
        public string GodzinaKoniec => Slot?.End.ToString("HH:mm") ?? "--:--";
        public bool JestDostepny => Slot?.Dostepny ?? false;
        public string Opis => Slot?.OpisKortu ?? "Brak opisu";
    }

    public class DostepneKortyViewModel : BaseViewModel
    {
        private readonly IKortRepository _kortRepo;
        private readonly IRezerwacjaService _rezerwacjaService;
        private readonly Uzytkownik _uzytkownik;

        private List<Kort> _wszystkieKorty;
        private ObservableCollection<Kort> _korty;
        private ObservableCollection<TypKortu> _dostepneTypyKortu;
        private ObservableCollection<ObiektSportowy> _dostepneObiekty;
        private ObservableCollection<OpcjaRezerwacji> _dostepneTerminy;

        private Kort _wybranyKort;
        private DateTime _wybranaData = DateTime.Today;
        private ObiektSportowy _wybranyObiektFiltr;
        private TypKortu _wybranyTypKortuFiltr;
        private decimal _maksymalnaCenaFiltr = 200;
        private int _godzinaOd = 8;
        private int _godzinaDo = 22;
        private ObservableCollection<decimal> _dostepneDlugosciSesji;
        private decimal _wybranaDlugoscSesji = 1.0m;

        public DostepneKortyViewModel(
            IKortRepository kortRepo,
            IRezerwacjaService rezerwacjaService,
            Uzytkownik uzytkownik)
        {
            _kortRepo = kortRepo ?? throw new ArgumentNullException(nameof(kortRepo));
            _rezerwacjaService = rezerwacjaService ?? throw new ArgumentNullException(nameof(rezerwacjaService));
            _uzytkownik = uzytkownik ?? throw new ArgumentNullException(nameof(uzytkownik));

            Korty = new ObservableCollection<Kort>();
            DostepneTerminy = new ObservableCollection<OpcjaRezerwacji>();
            DostepneTypyKortu = new ObservableCollection<TypKortu>();
            DostepneObiekty = new ObservableCollection<ObiektSportowy>();
            _wszystkieKorty = new List<Kort>();
            DostepneDlugosciSesji = new ObservableCollection<decimal> { 1.0m, 1.5m, 2.0m, 2.5m, 3.0m };

            ZaladujKortyCommand = new RelayCommand(async _ => await ZaladujKortyAsync());
            SzukajTerminowCommand = new RelayCommand(async _ => await SzukajTerminowAsync());
            RezerwujCommand = new RelayCommand(async param => await RezerwujAsync(param as OpcjaRezerwacji));
            ResetujFiltryCommand = new RelayCommand(_ => ZresetujFiltry());
            OtworzMapeCommand = new RelayCommand(param => OtworzLink(param as string));
            OtworzMapeZbiorczaCommand = new RelayCommand(_ => OtworzLink("https://www.google.com/maps/search/Korty+tenisowe+i+centra+sportowe+w+Bydgoszczy"));

            _ = ZaladujKortyAsync();
        }

        // --- WŁAŚCIWOŚCI ---
        public ObservableCollection<decimal> DostepneDlugosciSesji { get => _dostepneDlugosciSesji; set => SetProperty(ref _dostepneDlugosciSesji, value); }
        public decimal WybranaDlugoscSesji { get => _wybranaDlugoscSesji; set => SetProperty(ref _wybranaDlugoscSesji, value); }
        public int GodzinaOd { get => _godzinaOd; set => SetProperty(ref _godzinaOd, value); }
        public int GodzinaDo { get => _godzinaDo; set => SetProperty(ref _godzinaDo, value); }
        public ObservableCollection<Kort> Korty { get => _korty; set => SetProperty(ref _korty, value); }
        public ObservableCollection<TypKortu> DostepneTypyKortu { get => _dostepneTypyKortu; set => SetProperty(ref _dostepneTypyKortu, value); }
        public ObservableCollection<ObiektSportowy> DostepneObiekty { get => _dostepneObiekty; set => SetProperty(ref _dostepneObiekty, value); }
        public ObservableCollection<OpcjaRezerwacji> DostepneTerminy { get => _dostepneTerminy; set => SetProperty(ref _dostepneTerminy, value); }

        public ObiektSportowy WybranyObiektFiltr
        {
            get => _wybranyObiektFiltr;
            set { if (SetProperty(ref _wybranyObiektFiltr, value)) ZastosujFiltry(); }
        }

        public TypKortu WybranyTypKortuFiltr
        {
            get => _wybranyTypKortuFiltr;
            set { if (SetProperty(ref _wybranyTypKortuFiltr, value)) ZastosujFiltry(); }
        }

        public decimal MaksymalnaCenaFiltr
        {
            get => _maksymalnaCenaFiltr;
            set { if (SetProperty(ref _maksymalnaCenaFiltr, value)) ZastosujFiltry(); }
        }

        public Kort WybranyKort { get => _wybranyKort; set => SetProperty(ref _wybranyKort, value); }
        public DateTime WybranaData { get => _wybranaData; set => SetProperty(ref _wybranaData, value); }

        // --- KOMENDY ---
        public ICommand ZaladujKortyCommand { get; }
        public ICommand SzukajTerminowCommand { get; }
        public ICommand RezerwujCommand { get; }
        public ICommand ResetujFiltryCommand { get; }
        public ICommand OtworzMapeCommand { get; }
        public ICommand OtworzMapeZbiorczaCommand { get; }

        // --- METODY ---

        private void ZresetujFiltry()
        {
            WybranaData = DateTime.Today;
            WybranaDlugoscSesji = 1.0m;
            GodzinaOd = 8;
            GodzinaDo = 22;
            WybranyObiektFiltr = null;
            WybranyTypKortuFiltr = null;
            WybranyKort = null;

            // Odśwież wyniki po resecie
            _ = SzukajTerminowAsync();
        }

        private void OtworzLink(string link)
        {
            if (string.IsNullOrWhiteSpace(link)) return;
            try
            {
                Process.Start(new ProcessStartInfo { FileName = link, UseShellExecute = true });
            }
            catch (Exception ex) { MessageBox.Show($"Błąd mapy: {ex.Message}"); }
        }

        private async Task ZaladujKortyAsync()
        {
            try
            {
                var korty = await _kortRepo.GetAktywneKortyAsync();
                _wszystkieKorty = korty?.ToList() ?? new List<Kort>();

                var typy = _wszystkieKorty
                    .Select(k => k.TypKortu)
                    .Where(t => t != null)
                    .GroupBy(t => t.Id)
                    .Select(g => g.First())
                    .ToList();

                var obiekty = _wszystkieKorty
                    .Select(k => k.ObiektSportowy)
                    .Where(o => o != null)
                    .GroupBy(o => o.Id)
                    .Select(g => g.First())
                    .ToList();

                DostepneTypyKortu.Clear();
                foreach (var typ in typy) DostepneTypyKortu.Add(typ);

                DostepneObiekty.Clear();
                foreach (var ob in obiekty) DostepneObiekty.Add(ob);

                ZastosujFiltry();
                await SzukajTerminowAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd ładowania danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ZastosujFiltry()
        {
            if (_wszystkieKorty == null) return;
            var przefiltrowane = _wszystkieKorty.AsEnumerable();

            if (WybranyObiektFiltr != null) przefiltrowane = przefiltrowane.Where(k => k.ObiektSportowyId == WybranyObiektFiltr.Id);
            if (WybranyTypKortuFiltr != null) przefiltrowane = przefiltrowane.Where(k => k.TypKortuId == WybranyTypKortuFiltr.Id);

            przefiltrowane = przefiltrowane.Where(k => k.CenaZaGodzine <= MaksymalnaCenaFiltr);

            Korty.Clear();
            foreach (var kort in przefiltrowane) Korty.Add(kort);

            if (WybranyKort != null && !Korty.Contains(WybranyKort)) WybranyKort = null;
        }

        private async Task SzukajTerminowAsync()
        {
            if (WybranaData.Date < DateTime.Today)
            {
                MessageBox.Show("Nie można wybrać daty z przeszłości.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                DostepneTerminy.Clear();

                IEnumerable<Kort> kortyDoPrzeszukania = (WybranyKort != null)
                    ? new List<Kort> { WybranyKort }
                    : Korty;

                if (!kortyDoPrzeszukania.Any())
                {
                    return;
                }

                var wszystkieOpcje = new List<OpcjaRezerwacji>();

                foreach (var kort in kortyDoPrzeszukania)
                {
                    if (kort == null) continue;

                    var slotyKortu = await _rezerwacjaService.GetWolneTerminyAsync(
                        kort.Id,
                        WybranaData,
                        WybranaDlugoscSesji
                    );

                    // Pomijamy korty bez wolnych terminów
                    if (slotyKortu == null || !slotyKortu.Any())
                        continue;

                    foreach (var slot in slotyKortu)
                    {
                        // Sprawdzamy czy slot jest rzeczywiście dostępny
                        if (slot == null || !slot.Dostepny)
                            continue;

                        var obiekt = kort.ObiektSportowy;
                        string baseUrl = "pack://application:,,,/SRKT.WPF;component/images/korty/";
                        string nazwaPliku = "default_court.png";

                        if (!string.IsNullOrEmpty(kort.SciezkaZdjecia))
                        {
                            nazwaPliku = System.IO.Path.GetFileName(kort.SciezkaZdjecia);
                        }

                        string imgPath = baseUrl + nazwaPliku;

                        wszystkieOpcje.Add(new OpcjaRezerwacji
                        {
                            Slot = slot,
                            AdresObiektu = obiekt?.Adres ?? "Brak adresu",
                            NazwaObiektu = obiekt?.Nazwa ?? "Obiekt",
                            LinkDoMapy = obiekt?.LinkLokalizacji ?? "http://maps.google.com",
                            ZdjecieSciezka = imgPath,
                            CenaZaGodzine = kort.CenaZaGodzine
                        });
                    }
                }

                var przefiltrowane = wszystkieOpcje
                    .Where(o => o.Slot != null && o.Slot.Start.Hour >= GodzinaOd && o.Slot.End.Hour <= GodzinaDo)
                    .OrderBy(o => o.Slot.Start)
                    .ThenBy(o => o.NazwaObiektu)
                    .ThenBy(o => o.Slot.NazwaKortu);

                foreach (var opcja in przefiltrowane)
                    DostepneTerminy.Add(opcja);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd wyszukiwania terminów: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RezerwujAsync(OpcjaRezerwacji opcja)
        {
            // Walidacja
            if (opcja == null)
            {
                MessageBox.Show("Nie wybrano terminu do rezerwacji.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!opcja.JestDostepny)
            {
                MessageBox.Show("Wybrany termin nie jest dostępny.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (opcja.Slot == null)
            {
                MessageBox.Show("Brak danych o wybranym terminie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var window = new RezerwacjaWindow();

                // Pobierz serwis płatności z DI - bezpiecznie
                IPlatnoscService platnoscService = null;
                try
                {
                    var app = Application.Current as App;
                    if (app?.ServiceProvider != null)
                    {
                        platnoscService = app.ServiceProvider.GetService(typeof(IPlatnoscService)) as IPlatnoscService;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Nie można pobrać IPlatnoscService: {ex.Message}");
                }

                var vm = new RezerwacjaViewModel(
                    opcja,
                    _rezerwacjaService,
                    platnoscService, // Może być null - obsłużone w RezerwacjaViewModel
                    _uzytkownik,
                    () => window.Close()
                );

                window.DataContext = vm;

                // Bezpieczne ustawienie Owner
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow != null && mainWindow != window && mainWindow.IsLoaded)
                {
                    window.Owner = mainWindow;
                }

                window.ShowDialog();

                // Odśwież listę po zamknięciu okna rezerwacji
                await SzukajTerminowAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas otwierania okna rezerwacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}