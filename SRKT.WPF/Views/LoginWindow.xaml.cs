using Microsoft.Extensions.DependencyInjection;
using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.WPF.ViewModels;
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
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.SetUzytkownik(uzytkownik);
            mainWindow.Show();
            this.Close();
        }

        private void OnNavigateToRegister(object sender, EventArgs e)
        {
            var registerWindow = _serviceProvider.GetRequiredService<RegisterWindow>();

            registerWindow.Show();

            this.Close();
        }
    }
}