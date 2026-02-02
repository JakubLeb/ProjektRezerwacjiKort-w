using SRKT.Business.Services;
using SRKT.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SRKT.WPF.ViewModels
{
    public class MojeRezerwacjeViewModel : BaseViewModel
    {
        private readonly IRezerwacjaService _rezerwacjaService;
        private readonly Uzytkownik _uzytkownik;
        private ObservableCollection<Rezerwacja> _rezerwacje;
        private Rezerwacja _wybranaRezerwacja;
        private bool _isLoading;

        public MojeRezerwacjeViewModel(IRezerwacjaService rezerwacjaService, Uzytkownik uzytkownik)
        {
            _rezerwacjaService = rezerwacjaService ?? throw new ArgumentNullException(nameof(rezerwacjaService));
            _uzytkownik = uzytkownik ?? throw new ArgumentNullException(nameof(uzytkownik));

            Rezerwacje = new ObservableCollection<Rezerwacja>();

            ZaladujRezerwacjeCommand = new RelayCommand(async _ => await ZaladujRezerwacjeAsync(), _ => !IsLoading);
            AnulujRezerwacjeCommand = new RelayCommand(async param => await AnulujRezerwacjeAsync(param as Rezerwacja), _ => !IsLoading);

            _ = ZaladujRezerwacjeAsync();
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ObservableCollection<Rezerwacja> Rezerwacje
        {
            get => _rezerwacje;
            set => SetProperty(ref _rezerwacje, value);
        }

        public Rezerwacja WybranaRezerwacja
        {
            get => _wybranaRezerwacja;
            set => SetProperty(ref _wybranaRezerwacja, value);
        }

        public ICommand ZaladujRezerwacjeCommand { get; }
        public ICommand AnulujRezerwacjeCommand { get; }

        private async Task ZaladujRezerwacjeAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                var rezerwacje = await _rezerwacjaService.GetRezerwacjeUzytkownikaAsync(_uzytkownik.Id);

                // Aktualizuj na wątku UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Rezerwacje.Clear();

                    if (rezerwacje != null)
                    {
                        foreach (var rezerwacja in rezerwacje)
                        {
                            if (rezerwacja != null)
                            {
                                Rezerwacje.Add(rezerwacja);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Błąd ładowania rezerwacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AnulujRezerwacjeAsync(Rezerwacja rezerwacja)
        {
            // Jeśli przekazano parametr, użyj go; w przeciwnym razie użyj WybranaRezerwacja
            var rezerwacjaDoAnulowania = rezerwacja ?? WybranaRezerwacja;

            if (rezerwacjaDoAnulowania == null)
            {
                MessageBox.Show("Wybierz rezerwację do anulowania.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (IsLoading) return;

            var result = MessageBox.Show(
                $"Czy na pewno chcesz anulować tę rezerwację?\n\n" +
                $"Kort: {rezerwacjaDoAnulowania.Kort?.PelnaNazwa ?? "Nieznany"}\n" +
                $"Data: {rezerwacjaDoAnulowania.DataRezerwacji:dd.MM.yyyy HH:mm}",
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;

                    await _rezerwacjaService.AnulujRezerwacjeAsync(rezerwacjaDoAnulowania.Id);

                    MessageBox.Show("Rezerwacja została anulowana.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Odśwież listę po anulowaniu
                    await ZaladujRezerwacjeAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd anulowania rezerwacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }
}