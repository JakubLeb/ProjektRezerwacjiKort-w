using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SRKT.Business.Services;
using SRKT.DataAccess;
using SRKT.DataAccess.Repositories;
using SRKT.WPF.ViewModels;
using SRKT.WPF.Views;
using System;
using System.Windows;

namespace SRKT.WPF
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Connection string
            var connectionString = "Server=SCHIZOFRENIK\\SQLEXPRESS;Database=SystemRezerwacji;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;";
            // DbContext
            services.AddDbContext<SRKTDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IKortRepository, KortRepository>();
            services.AddScoped<IRezerwacjaRepository, RezerwacjaRepository>();

            //Service
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRezerwacjaService, RezerwacjaService>();
            services.AddScoped<IPowiadomienieService, PowiadomienieService>();

            // Views
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<RegisterWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Inicjalizacja bazy danych
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SRKTDbContext>();
                try
                {
                    dbContext.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd połączenia z bazą danych: {ex.Message}\n\nSprawdź connection string w App.xaml.cs",
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }
            }

            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        public ServiceProvider ServiceProvider => _serviceProvider;
    }
}