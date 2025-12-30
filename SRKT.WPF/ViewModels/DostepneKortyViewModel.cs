using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    public class DostepneKortyViewModel : BaseViewModel
    {
        private readonly IKortRepository _kortRepo;
        private readonly IRezerwacjaService _rezerwacjaService;
        private readonly Uzytkownik _uzytkownik;
        private ObservableCollection<Kort> _korty;
        private Kort _wybranyKort;
        private DateTime _wybranaData = DateTime.Today;
        private ObservableCollection<TimeSlot> _dostepneTerminy;

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

            ZaladujKortyCommand = new RelayCommand(async _ => await ZaladujKortyAsync());
            SzukajTerminowCommand = new RelayCommand(async _ => await SzukajTerminowAsync(), _ => WybranyKort != null);
            RezerwujCommand = new RelayCommand(async param => await RezerwujAsync(param as TimeSlot));

            _ = ZaladujKortyAsync();
        }

        public ObservableCollection<Kort> Korty
        {
            get => _korty;
            set => SetProperty(ref _korty, value);
        }

        public Kort WybranyKort
        {
            get => _wybranyKort;
            set
            {
                SetProperty(ref _wybranyKort, value);
                if (value != null)
                {
                    _ = SzukajTerminowAsync();
                }
            }
        }

        public DateTime WybranaData
        {
            get => _wybranaData;
            set
            {
                SetProperty(ref _wybranaData, value);
                if (WybranyKort != null)
                {
                    _ = SzukajTerminowAsync();
                }
            }
        }

        public ObservableCollection<TimeSlot> DostepneTerminy
        {
            get => _dostepneTerminy;
            set => SetProperty(ref _dostepneTerminy, value);
        }

        public ICommand ZaladujKortyCommand { get; }
        public ICommand SzukajTerminowCommand { get; }
        public ICommand RezerwujCommand { get; }

        private async Task ZaladujKortyAsync()
        {
            try
            {
                var korty = await _kortRepo.GetAktywneKortyAsync();
                Korty.Clear();
                foreach (var kort in korty)
                {
                    Korty.Add(kort);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd ładowania kortów: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SzukajTerminowAsync()
        {
            if (WybranyKort == null) return;

            try
            {
                var terminy = await _rezerwacjaService.GetWolneTerminyAsync(WybranyKort.Id, WybranaData);
                DostepneTerminy.Clear();
                foreach (var termin in terminy)
                {
                    DostepneTerminy.Add(termin);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd wyszukiwania terminów: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RezerwujAsync(TimeSlot slot)
        {
            if (slot == null || !slot.Dostepny || WybranyKort == null) return;

            try
            {
                await _rezerwacjaService.UtworzRezerwacjeAsync(
                    WybranyKort.Id,
                    _uzytkownik.Id,
                    slot.Start,
                    1.0m,
                    null
                );

                MessageBox.Show("Rezerwacja została utworzona!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                await SzukajTerminowAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd tworzenia rezerwacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}