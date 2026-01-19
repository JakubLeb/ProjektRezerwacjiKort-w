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
        private readonly MainViewModel _viewModel;

        public MainWindow(
            IKortRepository kortRepo,
            IRezerwacjaService rezerwacjaService,
            IRepository<Uzytkownik> uzytkownikRepo,
            IPowiadomienieService powiadomienieService)
        {
            InitializeComponent();

            // Pobierz serwis przypomnień z DI
            var przypomnienieService = ((App)Application.Current).ServiceProvider
                ?.GetService(typeof(IPrzypomnienieService)) as IPrzypomnienieService;

            _viewModel = new MainViewModel(
                kortRepo,
                rezerwacjaService,
                uzytkownikRepo,
                przypomnienieService,  // Może być null - MainViewModel to obsłuży
                powiadomienieService);

            _viewModel.LogoutRequested += OnLogoutRequested;

            DataContext = _viewModel;
        }

        public void SetUzytkownik(Uzytkownik uzytkownik)
        {
            _viewModel.AktualnyUzytkownik = uzytkownik;

            // Domyślnie pokaż dostępne korty
            _viewModel.PokazDostepneKortyCommand.Execute(null);
        }

        private void OnLogoutRequested(object sender, EventArgs e)
        {
            try
            {
                var app = (App)Application.Current;
                var loginWindow = app.ServiceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas wylogowywania: {ex.Message}",
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}