using Microsoft.Extensions.DependencyInjection;
using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using SRKT.WPF.ViewModels;
using SRKT.WPF.Views;
using System;
using System.Windows;

namespace SRKT.WPF
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        // AKTUALIZACJA: Dodano parametr 'uzytkownikRepo'
        public MainWindow(IKortRepository kortRepo, IRezerwacjaService rezerwacjaService, IRepository<Uzytkownik> uzytkownikRepo)
        {
            InitializeComponent();

            // Przekazujemy wszystkie 3 parametry do MainViewModel
            _viewModel = new MainViewModel(kortRepo, rezerwacjaService, uzytkownikRepo);
            _viewModel.LogoutRequested += OnLogoutRequested;

            DataContext = _viewModel;
        }

        public void SetUzytkownik(Uzytkownik uzytkownik)
        {
            _viewModel.AktualnyUzytkownik = uzytkownik;

            // Opcjonalnie: odśwież widok po zalogowaniu
            if (_viewModel.PokazDostepneKortyCommand.CanExecute(null))
            {
                _viewModel.PokazDostepneKortyCommand.Execute(null);
            }
        }

        private void OnLogoutRequested(object sender, EventArgs e)
        {
            var loginWindow = ((App)Application.Current).ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
            this.Close();
        }
    }
}