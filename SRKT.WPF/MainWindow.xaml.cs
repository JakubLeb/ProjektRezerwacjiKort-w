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

        public MainWindow(IKortRepository kortRepo, IRezerwacjaService rezerwacjaService)
        {
            InitializeComponent();

            _viewModel = new MainViewModel(kortRepo, rezerwacjaService);
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
            var loginWindow = ((App)Application.Current).ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}