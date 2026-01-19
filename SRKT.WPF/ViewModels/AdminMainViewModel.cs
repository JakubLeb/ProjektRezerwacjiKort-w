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
        private readonly IRezerwacjaRepository _rezerwacjaRepo;
        private readonly IRepository<ObiektSportowy> _obiektRepo;
        private readonly IRepository<Rola> _rolaRepo;

        private object _aktualnyWidok;
        private Uzytkownik _aktualnyAdministrator;

        public AdminMainViewModel(
            IRezerwacjaService rezerwacjaService,
            IKortRepository kortRepo,
            IRepository<Uzytkownik> uzytkownikRepo,
            IRezerwacjaRepository rezerwacjaRepo,
            IRepository<ObiektSportowy> obiektRepo,
            IRepository<Rola> rolaRepo = null)
        {
            _rezerwacjaService = rezerwacjaService;
            _kortRepo = kortRepo;
            _uzytkownikRepo = uzytkownikRepo;
            _rezerwacjaRepo = rezerwacjaRepo;
            _obiektRepo = obiektRepo;
            _rolaRepo = rolaRepo;

            PokazDashboardCommand = new RelayCommand(_ => PokazDashboard());
            PokazKalendarzCommand = new RelayCommand(_ => PokazKalendarz());
            PokazUzytkownikowCommand = new RelayCommand(_ => PokazUzytkownikow());
            WylogujCommand = new RelayCommand(_ => Wyloguj());

            PokazDashboard();
        }

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
        public ICommand PokazKalendarzCommand { get; }
        public ICommand PokazUzytkownikowCommand { get; }
        public ICommand WylogujCommand { get; }

        public event EventHandler LogoutRequested;

        private void PokazDashboard()
        {
            AktualnyWidok = new AdminDashboardView(_rezerwacjaService);
        }

        private void PokazKalendarz()
        {
            var viewModel = new AdminKalendarzViewModel(_rezerwacjaRepo, _kortRepo, _obiektRepo);
            var view = new AdminKalendarzView { DataContext = viewModel };
            AktualnyWidok = view;
        }

        private void PokazUzytkownikow()
        {
            if (_rolaRepo != null)
            {
                var viewModel = new AdminUzytkownicyViewModel(_uzytkownikRepo, _rolaRepo);
                var view = new AdminUzytkownicyView { DataContext = viewModel };
                AktualnyWidok = view;
            }
            else
            {
                // Fallback - pobierz rolaRepo z DI jeśli nie został przekazany
                var rolaRepo = ((App)Application.Current).ServiceProvider
                    .GetService(typeof(IRepository<Rola>)) as IRepository<Rola>;

                if (rolaRepo != null)
                {
                    var viewModel = new AdminUzytkownicyViewModel(_uzytkownikRepo, rolaRepo);
                    var view = new AdminUzytkownicyView { DataContext = viewModel };
                    AktualnyWidok = view;
                }
                else
                {
                    MessageBox.Show("Nie można załadować modułu użytkowników - brak serwisu ról.",
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Wyloguj()
        {
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}