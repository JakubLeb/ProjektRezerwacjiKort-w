using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess;
using SRKT.DataAccess.Repositories;
using SRKT.WPF.Services;
using SRKT.WPF.Views;
using System.Windows;

namespace SRKT.WPF
{
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }
        private ReminderBackgroundService _reminderService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Konfiguracja Dependency Injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Uruchom serwis sprawdzania przypomnień w tle
            StartReminderService();

            // Uruchom PIERWSZE okno logowania
            var loginWindow1 = ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow1.Show();

            // Uruchom DRUGIE okno logowania (opcjonalne - do testów)
            var loginWindow2 = ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow2.Title = "Logowanie - Okno 2";
            loginWindow2.Left = loginWindow1.Left + 50;
            loginWindow2.Top = loginWindow1.Top + 50;
            loginWindow2.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Zatrzymaj serwis przypomnień
            _reminderService?.Stop();
            _reminderService?.Dispose();

            base.OnExit(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Konfiguracja bazy danych
            services.AddDbContext<SRKTDbContext>(options =>
                options.UseSqlServer("Server=SCHIZOFRENIK\\SQLEXPRESS;Database=SystemRezerwacji;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;"));

            // Repozytoria
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IKortRepository, KortRepository>();
            services.AddScoped<IRezerwacjaRepository, RezerwacjaRepository>();

            // Serwisy biznesowe
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRezerwacjaService, RezerwacjaService>();
            services.AddScoped<IPlatnoscService, PlatnoscService>();

            // Serwisy powiadomień i przypomnień
            services.AddScoped<IRepository<Przypomnienie>, Repository<Przypomnienie>>();
            services.AddScoped<IPowiadomienieService, PowiadomienieService>();
            services.AddScoped<IPrzypomnienieService, PrzypomnienieService>();

            // Okna
            services.AddTransient<LoginWindow>();
            services.AddTransient<RegisterWindow>();
            services.AddTransient<MainWindow>(sp =>
            {
                var kortRepo = sp.GetRequiredService<IKortRepository>();
                var rezerwacjaService = sp.GetRequiredService<IRezerwacjaService>();
                var uzytkownikRepo = sp.GetRequiredService<IRepository<Uzytkownik>>();
                var powiadomienieService = sp.GetRequiredService<IPowiadomienieService>();
                return new MainWindow(kortRepo, rezerwacjaService, uzytkownikRepo, powiadomienieService);
            });
        }

        /// <summary>
        /// Uruchamia serwis sprawdzania przypomnień w tle
        /// </summary>
        private void StartReminderService()
        {
            try
            {
                var przypomnienieService = ServiceProvider.GetService<IPrzypomnienieService>();
                if (przypomnienieService != null)
                {
                    _reminderService = new ReminderBackgroundService(przypomnienieService);
                    _reminderService.Start();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd uruchamiania serwisu przypomnień: {ex.Message}");
            }
        }
    }
}