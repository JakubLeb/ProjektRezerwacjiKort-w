using SRKT.Core.Models;
using System.Windows;

namespace SRKT.WPF.Views
{
    public partial class EdytujUzytkownikaWindow : Window
    {
        public string NoweImie { get; private set; }
        public string NoweNazwisko { get; private set; }
        public string NowyEmail { get; private set; }

        public EdytujUzytkownikaWindow(Uzytkownik uzytkownik)
        {
            InitializeComponent();

            // Wypełnij pola aktualnymi danymi
            ImieTextBox.Text = uzytkownik.Imie;
            NazwiskoTextBox.Text = uzytkownik.Nazwisko;
            EmailTextBox.Text = uzytkownik.Email;

            // Ustaw tytuł okna
            Title = $"Edytuj: {uzytkownik.PelneImieNazwisko}";
        }

        private void ZapiszButton_Click(object sender, RoutedEventArgs e)
        {
            // Walidacja
            if (string.IsNullOrWhiteSpace(ImieTextBox.Text))
            {
                MessageBox.Show("Imię nie może być puste.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
                ImieTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(NazwiskoTextBox.Text))
            {
                MessageBox.Show("Nazwisko nie może być puste.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
                NazwiskoTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !EmailTextBox.Text.Contains("@"))
            {
                MessageBox.Show("Podaj poprawny adres email.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return;
            }

            // Zapisz wartości
            NoweImie = ImieTextBox.Text.Trim();
            NoweNazwisko = NazwiskoTextBox.Text.Trim();
            NowyEmail = EmailTextBox.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void AnulujButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}