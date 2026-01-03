using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace SRKT.WPF.ViewModels
{
    public class DostepneKortyViewModel : BaseViewModel
    {
        private readonly IKortRepository _kortRepo;
        private readonly IRezerwacjaService _rezerwacjaService;
        private readonly Uzytkownik _uzytkownik;

        private List<Kort> _wszystkieKorty;
        private ObservableCollection<Kort> _korty;
        private ObservableCollection<TypKortu> _dostepneTypyKortu;
        private ObservableCollection<ObiektSportowy> _dostepneObiekty;
        private ObservableCollection<TimeSlot> _dostepneTerminy;

        private Kort _wybranyKort;
        private DateTime _wybranaData = DateTime.Today;
        private ObiektSportowy _wybranyObiektFiltr;
        private TypKortu _wybranyTypKortuFiltr;
        private decimal _maksymalnaCenaFiltr = 200;
        private int _godzinaOd = 8;
        private int _godzinaDo = 22;

        private ObservableCollection<decimal> _dostepneDlugosciSesji;
        private decimal _wybranaDlugoscSesji = 1.0m; // Domyślnie 1h

        public DostepneKortyViewModel(
            IKortRepository kortRepo,
            IRezerwacjaService rezerwacjaService,
            Uzytkownik uzytkownik)
        {
            _kortRepo = kortRepo;
            _rezerwacjaService = rezerwacjaService;
            _uzytkownik = uzytkownik;

            Korty = new ObservableCollection<Kort>();
            DostepneTerminy = new ObservableCollection<TimeSlot>();
            DostepneTypyKortu = new ObservableCollection<TypKortu>();
            DostepneObiekty = new ObservableCollection<ObiektSportowy>();
            _wszystkieKorty = new List<Kort>();

            DostepneDlugosciSesji = new ObservableCollection<decimal> { 1.0m, 1.5m, 2.0m, 2.5m, 3.0m };

            ZaladujKortyCommand = new RelayCommand(async _ => await ZaladujKortyAsync());
            SzukajTerminowCommand = new RelayCommand(async _ => await SzukajTerminowAsync());
            RezerwujCommand = new RelayCommand(async param => await RezerwujAsync(param as TimeSlot));
            WyczyscFiltrObiektuCommand = new RelayCommand(_ => WybranyObiektFiltr = null);

            _ = ZaladujKortyAsync();
        }


        public ObservableCollection<decimal> DostepneDlugosciSesji
        {
            get => _dostepneDlugosciSesji;
            set => SetProperty(ref _dostepneDlugosciSesji, value);
        }

        public decimal WybranaDlugoscSesji
        {
            get => _wybranaDlugoscSesji;
            set
            {
                SetProperty(ref _wybranaDlugoscSesji, value);
            }
        }


        public int GodzinaOd
        {
            get => _godzinaOd;
            set => SetProperty(ref _godzinaOd, value);
        }

        public int GodzinaDo
        {
            get => _godzinaDo;
            set => SetProperty(ref _godzinaDo, value);
        }

        public ObservableCollection<Kort> Korty
        {
            get => _korty;
            set => SetProperty(ref _korty, value);
        }

        public ObservableCollection<TypKortu> DostepneTypyKortu
        {
            get => _dostepneTypyKortu;
            set => SetProperty(ref _dostepneTypyKortu, value);
        }

        public ObservableCollection<ObiektSportowy> DostepneObiekty
        {
            get => _dostepneObiekty;
            set => SetProperty(ref _dostepneObiekty, value);
        }

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

        public Kort WybranyKort
        {
            get => _wybranyKort;
            set => SetProperty(ref _wybranyKort, value);
        }

        public DateTime WybranaData
        {
            get => _wybranaData;
            set => SetProperty(ref _wybranaData, value);
        }

        public ObservableCollection<TimeSlot> DostepneTerminy
        {
            get => _dostepneTerminy;
            set => SetProperty(ref _dostepneTerminy, value);
        }

        public ICommand ZaladujKortyCommand { get; }
        public ICommand SzukajTerminowCommand { get; }
        public ICommand RezerwujCommand { get; }
        public ICommand WyczyscFiltrObiektuCommand { get; }

        private async Task ZaladujKortyAsync()
        {
            try
            {
                var korty = await _kortRepo.GetAktywneKortyAsync();
                _wszystkieKorty = korty.ToList();

                // Ładowanie filtrów 
                var typy = _wszystkieKorty.Select(k => k.TypKortu).Where(t => t != null).GroupBy(t => t.Id).Select(g => g.First()).ToList();
                var obiekty = _wszystkieKorty.Select(k => k.ObiektSportowy).Where(o => o != null).GroupBy(o => o.Id).Select(g => g.First()).ToList();

                DostepneTypyKortu.Clear();
                foreach (var typ in typy) DostepneTypyKortu.Add(typ);

                DostepneObiekty.Clear();
                foreach (var ob in obiekty) DostepneObiekty.Add(ob);

                ZastosujFiltry();

                // AUTOMATYCZNE WYSZUKIWANIE PO ZAŁADOWANIU DANYCH
                await SzukajTerminowAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd: {ex.Message}");
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
                MessageBox.Show("Nie można rezerwować terminów w przeszłości.", "Błąd daty");
                return;
            }

            try
            {
                DostepneTerminy.Clear();

                IEnumerable<Kort> kortyDoPrzeszukania;
                if (WybranyKort != null)
                    kortyDoPrzeszukania = new List<Kort> { WybranyKort };
                else
                    kortyDoPrzeszukania = Korty;

                var wszystkieSloty = new List<TimeSlot>();

                foreach (var kort in kortyDoPrzeszukania)
                {
                    // PRZEKAZANIE DŁUGOŚCI SESJI DO SERWISU
                    var slotyKortu = await _rezerwacjaService.GetWolneTerminyAsync(kort.Id, WybranaData, WybranaDlugoscSesji);
                    wszystkieSloty.AddRange(slotyKortu);
                }

                // Filtr godzinowy i sortowanie
                var przefiltrowaneSloty = wszystkieSloty
                    .Where(s => s.Start.Hour >= GodzinaOd && s.End.Hour <= GodzinaDo) // End.Hour może wyjść poza zakres jeśli sesja długa, można zmienić warunek na s.Start.Hour
                    .OrderBy(s => s.Start)
                    .ThenBy(s => s.NazwaKortu);

                foreach (var slot in przefiltrowaneSloty)
                {
                    DostepneTerminy.Add(slot);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd wyszukiwania: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RezerwujAsync(TimeSlot slot)
        {
            if (slot == null || !slot.Dostepny) return;

            try
            {
                await _rezerwacjaService.UtworzRezerwacjeAsync(
                    slot.KortId,
                    _uzytkownik.Id,
                    slot.Start,
                    slot.Dlugosc, 
                    null
                );

                MessageBox.Show($"Zarezerwowano: {slot.OpisKortu}\nGodz: {slot.Start:HH:mm} - {slot.End:HH:mm}\nCzas: {slot.Dlugosc}h", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                await SzukajTerminowAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd rezerwacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}