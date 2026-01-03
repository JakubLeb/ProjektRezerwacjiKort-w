using SRKT.Business.Services;
using SRKT.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SRKT.WPF.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly RegisterViewModel _viewModel;

        public RegisterWindow(IAuthService authService)
        {
            InitializeComponent();
            _viewModel = new RegisterViewModel(authService);
            _viewModel.RegistrationSuccessful += (s, e) => { CloseAndShowLogin(); };
            _viewModel.NavigateBack += (s, e) => { CloseAndShowLogin(); };

            DataContext = _viewModel;
        }

        private void CloseAndShowLogin()
        {
            var app = (App)Application.Current;
            var loginWindow = app.ServiceProvider.GetService(typeof(LoginWindow)) as Window;
            loginWindow.Show();
            this.Close();
        }

        private void HasloBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox box)
                _viewModel.Haslo = box.Password;
        }

        private void PotwierdzHasloBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox box)
                _viewModel.PotwierdzHaslo = box.Password;
        }
    }
}