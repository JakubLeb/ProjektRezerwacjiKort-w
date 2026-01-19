using SRKT.Business.Services;
using SRKT.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SRKT.WPF.Views
{
    public partial class AdminDashboardView : UserControl, INotifyPropertyChanged
    {
        private readonly IRezerwacjaService _service;

        public ObservableCollection<Rezerwacja> DzisiejszeRezerwacje { get; set; }
        public ObservableCollection<Rezerwacja> OczekujaceRezerwacje { get; set; }
        public DateTime DzisiejszaData { get; set; } = DateTime.Now;

        // Statystyki
        public int LiczbaOplaconych => DzisiejszeRezerwacje?.Count(r => r.CzyOplacona) ?? 0;
        public int LiczbaNieoplaconych => DzisiejszeRezerwacje?.Count(r => !r.CzyOplacona) ?? 0;

        // Widoczność sekcji oczekujących rezerwacji
        public Visibility CzyPokazacOczekujace =>
            (OczekujaceRezerwacje?.Count ?? 0) > 0 ? Visibility.Visible : Visibility.Collapsed;

        // Komendy
        public ICommand ZatwierdzRezerwacjeCommand { get; }
        public ICommand OdrzucRezerwacjeCommand { get; }
        public ICommand OznaczJakoOplaconeCommand { get; }

        public AdminDashboardView(IRezerwacjaService service)
        {
            InitializeComponent();
            _service = service;

            DzisiejszeRezerwacje = new ObservableCollection<Rezerwacja>();
            OczekujaceRezerwacje = new ObservableCollection<Rezerwacja>();

            // Komendy do zatwierdzania/odrzucania rezerwacji
            ZatwierdzRezerwacjeCommand = new ViewModels.RelayCommand(async param => await ZatwierdzRezerwacjeAsync(param as Rezerwacja));
            OdrzucRezerwacjeCommand = new ViewModels.RelayCommand(async param => await OdrzucRezerwacjeAsync(param as Rezerwacja));

            // NOWA KOMENDA: Oznacz jako opłacone (symulacja płatności gotówką)
            OznaczJakoOplaconeCommand = new ViewModels.RelayCommand(async param => await OznaczJakoOplaconeAsync(param as Rezerwacja));

            DataContext = this;
            ZaladujDane();
        }

        private async void ZaladujDane()
        {
            try
            {
                var dane = await _service.PobierzWszystkieRezerwacjeZDatyAsync(DateTime.Today);

                DzisiejszeRezerwacje.Clear();
                OczekujaceRezerwacje.Clear();

                foreach (var r in dane.OrderBy(r => r.DataRezerwacji))
                {
                    // Rezerwacje ze statusem 1 (Oczekująca) idą do sekcji oczekujących
                    if (r.StatusRezerwacjiId == 1)
                    {
                        OczekujaceRezerwacje.Add(r);
                    }

                    // Wszystkie rezerwacje (oprócz anulowanych) pokazujemy w głównej tabeli
                    if (r.StatusRezerwacjiId != 3)
                    {
                        DzisiejszeRezerwacje.Add(r);
                    }
                }

                OdswiezStatystyki();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd ładowania danych: {ex.Message}");
            }
        }

        private async Task ZatwierdzRezerwacjeAsync(Rezerwacja rezerwacja)
        {
            if (rezerwacja == null) return;

            string uwagiInfo = string.IsNullOrWhiteSpace(rezerwacja.Uwagi)
                ? ""
                : $"\nUwagi: {rezerwacja.Uwagi}";

            var result = MessageBox.Show(
                $"Czy na pewno chcesz ZATWIERDZIĆ rezerwację?\n\n" +
                $"Klient: {rezerwacja.Uzytkownik?.PelneImieNazwisko ?? "Nieznany"}\n" +
                $"Kort: {rezerwacja.Kort?.Nazwa ?? "Nieznany"}\n" +
                $"Godzina: {rezerwacja.DataRezerwacji:HH:mm} - {rezerwacja.DataZakonczenia:HH:mm}" +
                uwagiInfo,
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var sukces = await _service.PotwierdRezerwacjeAsync(rezerwacja.Id);

                    if (sukces)
                    {
                        MessageBox.Show("Rezerwacja została zatwierdzona.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Pełne odświeżenie danych z bazy
                        ZaladujDane();
                    }
                    else
                    {
                        MessageBox.Show("Nie udało się zatwierdzić rezerwacji.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task OdrzucRezerwacjeAsync(Rezerwacja rezerwacja)
        {
            if (rezerwacja == null) return;

            string uwagiInfo = string.IsNullOrWhiteSpace(rezerwacja.Uwagi)
                ? ""
                : $"\nUwagi: {rezerwacja.Uwagi}";

            var result = MessageBox.Show(
                $"Czy na pewno chcesz ANULOWAĆ rezerwację?\n\n" +
                $"Klient: {rezerwacja.Uzytkownik?.PelneImieNazwisko ?? "Nieznany"}\n" +
                $"Kort: {rezerwacja.Kort?.Nazwa ?? "Nieznany"}\n" +
                $"Godzina: {rezerwacja.DataRezerwacji:HH:mm} - {rezerwacja.DataZakonczenia:HH:mm}" +
                uwagiInfo,
                "Potwierdzenie anulowania",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var sukces = await _service.AnulujRezerwacjeAsync(rezerwacja.Id);

                    if (sukces)
                    {
                        MessageBox.Show("Rezerwacja została anulowana.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Pełne odświeżenie danych z bazy
                        ZaladujDane();
                    }
                    else
                    {
                        MessageBox.Show("Nie udało się anulować rezerwacji.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Oznacza rezerwację jako opłaconą - symulacja przyjęcia płatności gotówką na miejscu
        /// </summary>
        private async Task OznaczJakoOplaconeAsync(Rezerwacja rezerwacja)
        {
            if (rezerwacja == null) return;

            // Sprawdź czy już nie jest opłacona
            if (rezerwacja.CzyOplacona)
            {
                MessageBox.Show("Ta rezerwacja jest już opłacona.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Czy potwierdzasz przyjęcie płatności gotówką?\n\n" +
                $"Klient: {rezerwacja.Uzytkownik?.PelneImieNazwisko ?? "Nieznany"}\n" +
                $"Kort: {rezerwacja.Kort?.Nazwa ?? "Nieznany"}\n" +
                $"Godzina: {rezerwacja.DataRezerwacji:HH:mm} - {rezerwacja.DataZakonczenia:HH:mm}\n" +
                $"Kwota: {rezerwacja.KosztCalkowity:N2} zł",
                "Potwierdzenie płatności",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var sukces = await _service.OznaczJakoOplaconeAsync(rezerwacja.Id);

                    if (sukces)
                    {
                        MessageBox.Show(
                            $"Płatność została przyjęta!\n\nKwota: {rezerwacja.KosztCalkowity:N2} zł",
                            "Sukces",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        // Odśwież dane
                        ZaladujDane();
                    }
                    else
                    {
                        MessageBox.Show("Nie udało się oznaczyć rezerwacji jako opłaconej.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OdswiezStatystyki()
        {
            OnPropertyChanged(nameof(LiczbaOplaconych));
            OnPropertyChanged(nameof(LiczbaNieoplaconych));
            OnPropertyChanged(nameof(DzisiejszeRezerwacje));
            OnPropertyChanged(nameof(OczekujaceRezerwacje));
            OnPropertyChanged(nameof(CzyPokazacOczekujace));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}