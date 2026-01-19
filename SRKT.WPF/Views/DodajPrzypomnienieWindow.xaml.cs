using SRKT.Business.Services;
using SRKT.Core.Models;
using System.Windows;
using System.Windows.Controls;

namespace SRKT.WPF.Views
{
    public partial class DodajPrzypomnienieWindow : Window
    {
        private readonly List<Rezerwacja> _rezerwacje;
        private readonly IPrzypomnienieService _przypomnienieService;
        private Rezerwacja _wybranaRezerwacja;

        public DodajPrzypomnienieWindow(List<Rezerwacja> rezerwacje, IPrzypomnienieService przypomnienieService)
        {
            InitializeComponent();

            _rezerwacje = rezerwacje;
            _przypomnienieService = przypomnienieService;

            // Wypełnij ComboBox rezerwacjami
            RezerwacjaComboBox.ItemsSource = _rezerwacje;
            if (_rezerwacje.Any())
            {
                RezerwacjaComboBox.SelectedIndex = 0;
            }

            DataPicker.SelectedDate = DateTime.Today;
            AktualizujPodgladCzasu();
        }

        private void RezerwacjaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _wybranaRezerwacja = RezerwacjaComboBox.SelectedItem as Rezerwacja;
            
            if (_wybranaRezerwacja != null)
            {
                // Ustaw domyślną treść
                TrescTextBox.Text = $"Za chwilę rozpoczyna się Twoja rezerwacja kortu.\n" +
                                   $"Data: {_wybranaRezerwacja.DataRezerwacji:dd.MM.yyyy HH:mm}\n" +
                                   $"Czas trwania: {_wybranaRezerwacja.IloscGodzin}h";
                
                AktualizujPodgladCzasu();
            }
        }

        private void RadioCzas_Checked(object sender, RoutedEventArgs e)
        {
            if (CustomTimePanel == null) return;

            CustomTimePanel.Visibility = RadioCustom.IsChecked == true 
                ? Visibility.Visible 
                : Visibility.Collapsed;

            AktualizujPodgladCzasu();
        }

        private void AktualizujPodgladCzasu()
        {
            if (_wybranaRezerwacja == null || PodgladCzasuText == null) return;

            var dataPrzypomnienia = ObliczDatePrzypomnienia();
            
            if (dataPrzypomnienia.HasValue)
            {
                PodgladCzasuText.Text = dataPrzypomnienia.Value.ToString("dd.MM.yyyy HH:mm");
            }
            else
            {
                PodgladCzasuText.Text = "Nieprawidłowa data";
            }
        }

        private DateTime? ObliczDatePrzypomnienia()
        {
            if (_wybranaRezerwacja == null) return null;

            if (Radio15min?.IsChecked == true)
            {
                return _wybranaRezerwacja.DataRezerwacji.AddMinutes(-15);
            }
            else if (Radio30min?.IsChecked == true)
            {
                return _wybranaRezerwacja.DataRezerwacji.AddMinutes(-30);
            }
            else if (Radio1h?.IsChecked == true)
            {
                return _wybranaRezerwacja.DataRezerwacji.AddHours(-1);
            }
            else if (RadioCustom?.IsChecked == true)
            {
                if (DataPicker.SelectedDate.HasValue && TimeSpan.TryParse(CzasTextBox.Text, out var czas))
                {
                    return DataPicker.SelectedDate.Value.Date.Add(czas);
                }
            }

            return null;
        }

        private async void Zapisz_Click(object sender, RoutedEventArgs e)
        {
            // Walidacja
            if (_wybranaRezerwacja == null)
            {
                MessageBox.Show("Wybierz rezerwację.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TytulTextBox.Text))
            {
                MessageBox.Show("Podaj tytuł przypomnienia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                TytulTextBox.Focus();
                return;
            }

            var dataPrzypomnienia = ObliczDatePrzypomnienia();
            if (!dataPrzypomnienia.HasValue)
            {
                MessageBox.Show("Nieprawidłowa data przypomnienia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dataPrzypomnienia.Value <= DateTime.Now)
            {
                MessageBox.Show("Data przypomnienia musi być w przyszłości.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dataPrzypomnienia.Value >= _wybranaRezerwacja.DataRezerwacji)
            {
                MessageBox.Show("Data przypomnienia musi być przed datą rezerwacji.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _przypomnienieService.UtworzPrzypomnienieAsync(
                    _wybranaRezerwacja.Id,
                    _wybranaRezerwacja.UzytkownikId,
                    dataPrzypomnienia.Value,
                    TytulTextBox.Text.Trim(),
                    TrescTextBox.Text.Trim());

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd zapisywania: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Anuluj_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
