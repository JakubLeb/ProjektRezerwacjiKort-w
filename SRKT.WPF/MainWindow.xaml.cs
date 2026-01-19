using Microsoft.Extensions.DependencyInjection;
using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using SRKT.WPF.ViewModels;
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

            // Pobierz IPrzypomnienieService z DI
            var przypomnienieService = ((App)Application.Current).ServiceProvider
                .GetService(typeof(IPrzypomnienieService)) as IPrzypomnienieService;

            // WAŻNE: Prawidłowa kolejność argumentów!
            // MainViewModel(kortRepo, rezerwacjaService, uzytkownikRepo, przypomnienieService, powiadomienieService)
            _viewModel = new MainViewModel(
                kortRepo,
                rezerwacjaService,
                uzytkownikRepo,
                przypomnienieService,      // 4. argument - IPrzypomnienieService
                powiadomienieService);     // 5. argument - IPowiadomienieService

            _viewModel.LogoutRequested += OnLogoutRequested;

            DataContext = _viewModel;
        }

        public void SetUzytkownik(Uzytkownik uzytkownik)
        {
            _viewModel.AktualnyUzytkownik = uzytkownik;
            _viewModel.PokazDostepneKortyCommand.Execute(null);
        }

        private void OnLogoutRequested(object sender, EventArgs e)
        {
            var app = (App)Application.Current;
            var loginWindow = app.ServiceProvider.GetRequiredService<Views.LoginWindow>();
            loginWindow.Show();
            this.Close();
        }
    }
}