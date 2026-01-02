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

        public MojeRezerwacjeViewModel(IRezerwacjaService rezerwacjaService, Uzytkownik uzytkownik)
        {
            _rezerwacjaService = rezerwacjaService;
            _uzytkownik = uzytkownik;

            Rezerwacje = new ObservableCollection<Rezerwacja>();

            ZaladujRezerwacjeCommand = new RelayCommand(async _ => await ZaladujRezerwacjeAsync());
            AnulujRezerwacjeCommand = new RelayCommand(async _ => await AnulujRezerwacjeAsync(), _ => WybranaRezerwacja != null);

            _ = ZaladujRezerwacjeAsync();
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
            try
            {
                var rezerwacje = await _rezerwacjaService.GetRezerwacjeUzytkownikaAsync(_uzytkownik.Id);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd ładowania rezerwacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AnulujRezerwacjeAsync()
        {
            if (WybranaRezerwacja == null) return;

            var result = MessageBox.Show(
                "Czy na pewno chcesz anulować tę rezerwację?",
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _rezerwacjaService.AnulujRezerwacjeAsync(WybranaRezerwacja.Id);
                    MessageBox.Show("Rezerwacja została anulowana.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    await ZaladujRezerwacjeAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd anulowania rezerwacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}