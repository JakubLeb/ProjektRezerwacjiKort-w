using SRKT.Business.Services;
using SRKT.Core.Models;
using System.Windows;

namespace SRKT.WPF.Views
{
    public partial class EdytujPrzypomnienieWindow : Window
    {
        private readonly Przypomnienie _przypomnienie;
        private readonly IPrzypomnienieService _przypomnienieService;

        public EdytujPrzypomnienieWindow(Przypomnienie przypomnienie, IPrzypomnienieService przypomnienieService)
        {
            InitializeComponent();

            _przypomnienie = przypomnienie;
            _przypomnienieService = przypomnienieService;

            // Wypełnij pola aktualnymi danymi
            DataPicker.SelectedDate = przypomnienie.DataPrzypomnienia.Date;
            CzasTextBox.Text = przypomnienie.DataPrzypomnienia.ToString("HH:mm");
            TytulTextBox.Text = przypomnienie.Tytul;
            TrescTextBox.Text = przypomnienie.Tresc;
        }

        private async void Zapisz_Click(object sender, RoutedEventArgs e)
        {
            // Walidacja
            if (string.IsNullOrWhiteSpace(TytulTextBox.Text))
            {
                MessageBox.Show("Podaj tytuł przypomnienia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                TytulTextBox.Focus();
                return;
            }

            if (!DataPicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Wybierz datę.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParse(CzasTextBox.Text, out var czas))
            {
                MessageBox.Show("Podaj poprawny czas w formacie HH:mm", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                CzasTextBox.Focus();
                return;
            }

            var nowaData = DataPicker.SelectedDate.Value.Date.Add(czas);

            if (nowaData <= DateTime.Now)
            {
                MessageBox.Show("Data przypomnienia musi być w przyszłości.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var sukces = await _przypomnienieService.AktualizujPrzypomnienieAsync(
                    _przypomnienie.Id,
                    nowaData,
                    TytulTextBox.Text.Trim(),
                    TrescTextBox.Text.Trim());

                if (sukces)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Nie udało się zaktualizować przypomnienia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Anuluj_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
