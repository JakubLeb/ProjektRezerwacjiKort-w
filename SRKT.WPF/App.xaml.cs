using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SRKT.Business.Services;
using SRKT.Core.Models;
using SRKT.DataAccess;
using SRKT.DataAccess.Repositories;
using SRKT.WPF.Views;
using System.Windows;

namespace SRKT.WPF
{
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Konfiguracja Dependency Injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Uruchom okno logowania
            var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Konfiguracja bazy danych - zmień connection string na swój
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

            // ⭐ Serwis powiadomień - zarejestrowany jako Singleton dla obsługi eventów Toast
            services.AddSingleton<IPowiadomienieService, PowiadomienieService>();

            // Serwis przypomnień
            services.AddScoped<IPrzypomnienieService, PrzypomnienieService>();
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
    }
}
