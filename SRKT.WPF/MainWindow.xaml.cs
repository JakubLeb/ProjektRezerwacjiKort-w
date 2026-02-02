using Microsoft.Extensions.DependencyInjection;
using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using SRKT.WPF.ViewModels;
using SRKT.WPF.Views;
using System.Windows;

namespace SRKT.WPF
{
    public partial class MainWindow : Window
    {
        private readonly IKortRepository _kortRepo;
        private readonly IRezerwacjaService _rezerwacjaService;
        private readonly IRepository<Uzytkownik> _uzytkownikRepo;
        private readonly IPowiadomienieService _powiadomienieService;
        private MainViewModel _viewModel;

        public MainWindow(
            IKortRepository kortRepo,
            IRezerwacjaService rezerwacjaService,
            IRepository<Uzytkownik> uzytkownikRepo,
            IPowiadomienieService powiadomienieService)
        {
            InitializeComponent();

            _kortRepo = kortRepo;
            _rezerwacjaService = rezerwacjaService;
            _uzytkownikRepo = uzytkownikRepo;
            _powiadomienieService = powiadomienieService;

            // NIE tworzymy jeszcze ViewModelu - czekamy na SetUzytkownik
        }

        /// <summary>
        /// Ustawia zalogowanego użytkownika i inicjalizuje ViewModel
        /// </summary>
        public void SetUzytkownik(Uzytkownik uzytkownik)
        {
            if (uzytkownik == null)
            {
                MessageBox.Show("Błąd: Nie przekazano danych użytkownika.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // Pobierz serwis przypomnień z DI
            IPrzypomnienieService przypomnienieService = null;
            try
            {
                var app = Application.Current as App;
                if (app?.ServiceProvider != null)
                {
                    przypomnienieService = app.ServiceProvider.GetService(typeof(IPrzypomnienieService)) as IPrzypomnienieService;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Nie można pobrać IPrzypomnienieService: {ex.Message}");
            }

            // Teraz tworzymy ViewModel z użytkownikiem
            _viewModel = new MainViewModel(
                _kortRepo,
                _rezerwacjaService,
                _uzytkownikRepo,
                przypomnienieService,
                _powiadomienieService);

            _viewModel.AktualnyUzytkownik = uzytkownik;

            // Obsługa wylogowania
            _viewModel.LogoutRequested += (s, e) =>
            {
                var app = Application.Current as App;
                if (app?.ServiceProvider != null)
                {
                    var loginWindow = app.ServiceProvider.GetRequiredService<LoginWindow>();
                    loginWindow.Show();
                }
                Close();
            };

            DataContext = _viewModel;

            // Pokaż domyślny widok PO ustawieniu użytkownika
            _viewModel.PokazDostepneKortyCommand.Execute(null);
        }
    }
}