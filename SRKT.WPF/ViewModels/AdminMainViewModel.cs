using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using SRKT.WPF.Views;
using System;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;

namespace SRKT.WPF.ViewModels
{
    public class AdminMainViewModel : BaseViewModel
    {
        private readonly IRezerwacjaService _rezerwacjaService;
        private readonly IKortRepository _kortRepo;
        private readonly IRepository<Uzytkownik> _uzytkownikRepo;

        private object _aktualnyWidok;

        // DODANO: Pole dla administratora
        private Uzytkownik _aktualnyAdministrator;

        public AdminMainViewModel(
            IRezerwacjaService rezerwacjaService,
            IKortRepository kortRepo,
            IRepository<Uzytkownik> uzytkownikRepo)
        {
            _rezerwacjaService = rezerwacjaService;
            _kortRepo = kortRepo;
            _uzytkownikRepo = uzytkownikRepo;

            PokazDashboardCommand = new RelayCommand(_ => PokazDashboard());
            PokazKortyCommand = new RelayCommand(_ => PokazKorty());
            PokazRezerwacjeCommand = new RelayCommand(_ => PokazWszystkieRezerwacje());
            PokazUzytkownikowCommand = new RelayCommand(_ => PokazUzytkownikow());
            WylogujCommand = new RelayCommand(_ => Wyloguj());

            PokazDashboard();
        }

        // DODANO: Właściwość publiczna
        public Uzytkownik AktualnyAdministrator
        {
            get => _aktualnyAdministrator;
            set => SetProperty(ref _aktualnyAdministrator, value);
        }

        public object AktualnyWidok
        {
            get => _aktualnyWidok;
            set => SetProperty(ref _aktualnyWidok, value);
        }

        public ICommand PokazDashboardCommand { get; }
        public ICommand PokazKortyCommand { get; }
        public ICommand PokazRezerwacjeCommand { get; }
        public ICommand PokazUzytkownikowCommand { get; }
        public ICommand WylogujCommand { get; }

        public event EventHandler LogoutRequested;

        private void PokazDashboard()
        {
            AktualnyWidok = new AdminDashboardView(_rezerwacjaService);
        }

        private void PokazKorty()
        {
            AktualnyWidok = new TextBlock
            {
                Text = "Zarządzanie Kortami - w budowie",
                FontSize = 20,
                Margin = new Thickness(20)
            };
        }

        private void PokazWszystkieRezerwacje()
        {
            AktualnyWidok = new TextBlock
            {
                Text = "Zarządzanie Rezerwacjami - w budowie",
                FontSize = 20,
                Margin = new Thickness(20)
            };
        }

        private void PokazUzytkownikow()
        {
            AktualnyWidok = new TextBlock
            {
                Text = "Zarządzanie Użytkownikami - w budowie",
                FontSize = 20,
                Margin = new Thickness(20)
            };
        }

        private void Wyloguj()
        {
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}