using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IKortRepository _kortRepo;
        private readonly IRezerwacjaService _rezerwacjaService;
        private Uzytkownik _aktualnyUzytkownik;
        private object _aktualnyWidok;

        public MainViewModel(
            IKortRepository kortRepo,
            IRezerwacjaService rezerwacjaService)
        {
            _kortRepo = kortRepo;
            _rezerwacjaService = rezerwacjaService;

            PokazDostepneKortyCommand = new RelayCommand(_ => PokazDostepneKorty());
            PokazMojeRezerwacjeCommand = new RelayCommand(_ => PokazMojeRezerwacje());
            WylogujCommand = new RelayCommand(_ => Wyloguj());
        }

        public Uzytkownik AktualnyUzytkownik
        {
            get => _aktualnyUzytkownik;
            set => SetProperty(ref _aktualnyUzytkownik, value);
        }

        public object AktualnyWidok
        {
            get => _aktualnyWidok;
            set => SetProperty(ref _aktualnyWidok, value);
        }

        public ICommand PokazDostepneKortyCommand { get; }
        public ICommand PokazMojeRezerwacjeCommand { get; }
        public ICommand WylogujCommand { get; }

        public event EventHandler LogoutRequested;

        private void PokazDostepneKorty()
        {
            var viewModel = new DostepneKortyViewModel(_kortRepo, _rezerwacjaService, AktualnyUzytkownik);
            AktualnyWidok = viewModel;
        }

        private void PokazMojeRezerwacje()
        {
            var viewModel = new MojeRezerwacjeViewModel(_rezerwacjaService, AktualnyUzytkownik);
            AktualnyWidok = viewModel;
        }

        private void Wyloguj()
        {
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}