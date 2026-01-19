using Microsoft.Extensions.DependencyInjection;
using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using SRKT.WPF.Services;
using SRKT.WPF.ViewModels;
using System;
using System.Windows;

namespace SRKT.WPF
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private PrzypomnienieBackgroundService _przypomnienieBackgroundService;

        public MainWindow(
            IKortRepository kortRepo,
            IRezerwacjaService rezerwacjaService,
            IRepository<Uzytkownik> uzytkownikRepo,
            IPowiadomienieService powiadomienieService)
        {
            InitializeComponent();

            // Pobierz serwis przypomnień z DI
            var przypomnienieService = ((App)Application.Current).ServiceProvider
                .GetService(typeof(IPrzypomnienieService)) as IPrzypomnienieService;

            _viewModel = new MainViewModel(
                kortRepo,
                rezerwacjaService,
                uzytkownikRepo,
                przypomnienieService,
                powiadomienieService);

            _viewModel.LogoutRequested += OnLogoutRequested;

            DataContext = _viewModel;

            // Inicjalizuj ToastManager z kontenerem
            ToastManager.Instance.Initialize(ToastContainer);

            // Pokaż domyślny widok
            _viewModel.PokazDostepneKortyCommand.Execute(null);
        }

        public void SetUzytkownik(Uzytkownik uzytkownik)
        {
            _viewModel.AktualnyUzytkownik = uzytkownik;

            // Uruchom serwis sprawdzania przypomnień
            StartPrzypomnienieService(uzytkownik.Id);

            // Wyświetl toast powitalny
            ToastManager.Instance.ShowSuccess(
                "Witaj!",
                $"Zalogowano jako {uzytkownik.PelneImieNazwisko}");
        }

        private void StartPrzypomnienieService(int uzytkownikId)
        {
            try
            {
                var serviceProvider = ((App)Application.Current).ServiceProvider;
                var przypomnienieService = serviceProvider.GetService(typeof(IPrzypomnienieService)) as IPrzypomnienieService;
                var powiadomienieService = serviceProvider.GetService(typeof(IPowiadomienieService)) as IPowiadomienieService;

                if (przypomnienieService != null && powiadomienieService != null)
                {
                    _przypomnienieBackgroundService = new PrzypomnienieBackgroundService(
                        przypomnienieService,
                        powiadomienieService,
                        uzytkownikId);

                    _przypomnienieBackgroundService.Start();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd uruchamiania serwisu przypomnień: {ex.Message}");
            }
        }

        private void OnLogoutRequested(object sender, EventArgs e)
        {
            // Zatrzymaj serwis przypomnień
            _przypomnienieBackgroundService?.Stop();
            _przypomnienieBackgroundService?.Dispose();

            var serviceProvider = ((App)Application.Current).ServiceProvider;
            var loginWindow = serviceProvider.GetRequiredService<Views.LoginWindow>();
            loginWindow.Show();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Zatrzymaj serwis przy zamykaniu okna
            _przypomnienieBackgroundService?.Stop();
            _przypomnienieBackgroundService?.Dispose();

            base.OnClosed(e);
        }
    }
}