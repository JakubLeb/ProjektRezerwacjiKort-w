using Microsoft.Extensions.DependencyInjection;
using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess.Repositories; // Ważne: dodaj ten using dla repozytoriów
using SRKT.WPF.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SRKT.WPF.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;
        private readonly IServiceProvider _serviceProvider;

        public LoginWindow(IAuthService authService)
        {
            InitializeComponent();

            // Pobieramy ServiceProvider z aplikacji, aby móc tworzyć inne okna i serwisy
            _serviceProvider = ((App)Application.Current).ServiceProvider;
            
            _viewModel = new LoginViewModel(authService);
            _viewModel.LoginSuccessful += OnLoginSuccessful;
            _viewModel.NavigateToRegister += OnNavigateToRegister;

            DataContext = _viewModel;
        }

        private void HasloPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.Haslo = passwordBox.Password;
            }
        }

        private void OnLoginSuccessful(object sender, Uzytkownik uzytkownik)
        {
            // 1. Pobieramy serwisy z DI
            var rezerwacjaService = _serviceProvider.GetRequiredService<IRezerwacjaService>();
            var kortRepo = _serviceProvider.GetRequiredService<IKortRepository>();
            var uzytkownikRepo = _serviceProvider.GetRequiredService<IRepository<Uzytkownik>>();

            if (uzytkownik.RolaId == 1) // ADMIN
            {
                var adminVM = new AdminMainViewModel(rezerwacjaService, kortRepo, uzytkownikRepo);
                adminVM.AktualnyAdministrator = uzytkownik;
                var adminWindow = new AdminWindow();
                adminWindow.DataContext = adminVM;

                adminVM.LogoutRequested += (s, args) =>
                {
                    OtworzPonownieLogowanie(adminWindow);
                };

                adminWindow.Show();
            }
            else // ZWYKŁY UŻYTKOWNIK
            {
                // Tworzymy MainWindow, przekazując mu wymagane serwisy (w tym uzytkownikRepo!)
                var mainWindow = new MainWindow(kortRepo, rezerwacjaService, uzytkownikRepo);

                // Ustawiamy użytkownika (MainWindow samo ustawi to w swoim ViewModelu)
                mainWindow.SetUzytkownik(uzytkownik);

                // Nie musimy ręcznie tworzyć MainViewModel ani przypisywać DataContext,
                // ponieważ MainWindow robi to teraz w swoim konstruktorze.

                mainWindow.Show();
            }

            this.Close();
        }

        private void OtworzPonownieLogowanie(Window currentWindow)
        {
            // Pobieramy nową instancję LoginWindow z DI (aby wstrzyknąć IAuthService)
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
            currentWindow.Close();
        }

        private void OnNavigateToRegister(object sender, EventArgs e)
        {
            var registerWindow = _serviceProvider.GetRequiredService<RegisterWindow>();
            registerWindow.Show();
            this.Close();
        }
    }
}