using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess.Repositories;
using SRKT.WPF.Services;
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
            IPowiadomienieService powiadomienieService = null)
        {
            InitializeComponent();

            _viewModel = new MainViewModel(kortRepo, rezerwacjaService, uzytkownikRepo, powiadomienieService);
            _viewModel.LogoutRequested += OnLogoutRequested;

            DataContext = _viewModel;

            // Inicjalizuj ToastManager z kontenerem
            ToastManager.Instance.Initialize(ToastContainer);

            // Ustaw domyślny widok
            Loaded += MainWindow_Loaded;
        }

        // Konstruktor zachowany dla kompatybilności wstecznej
        public MainWindow(
            IKortRepository kortRepo,
            IRezerwacjaService rezerwacjaService,
            IRepository<Uzytkownik> uzytkownikRepo)
            : this(kortRepo, rezerwacjaService, uzytkownikRepo, null)
        {
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Pokaż domyślny widok (Dostępne Korty)
            _viewModel.PokazDostepneKortyCommand.Execute(null);
        }

        public void SetUzytkownik(Uzytkownik uzytkownik)
        {
            _viewModel.AktualnyUzytkownik = uzytkownik;
        }

        private void OnLogoutRequested(object sender, EventArgs e)
        {
            // Wyczyść Toasty
            ToastManager.Instance.ClearAll();

            // Otwórz okno logowania
            var app = (App)Application.Current;
            var loginWindow = app.ServiceProvider.GetService(typeof(LoginWindow)) as Window;
            loginWindow?.Show();

            this.Close();
        }

        /// <summary>
        /// Wyświetla Toast notification bezpośrednio z okna
        /// </summary>
        public void ShowToast(string tytul, string tresc)
        {
            ToastManager.Instance.ShowInfo(tytul, tresc);
        }
    }
}
