using SRKT.Business.Services;
using SRKT.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace SRKT.WPF.Views
{
    public partial class AdminDashboardView : UserControl, INotifyPropertyChanged
    {
        private readonly IRezerwacjaService _service;
        public ObservableCollection<Rezerwacja> DzisiejszeRezerwacje { get; set; }
        public DateTime DzisiejszaData { get; set; } = DateTime.Now;

        // Statystyki obliczane na bieżąco
        public int LiczbaOplaconych => DzisiejszeRezerwacje?.Count(r => r.CzyOplacona) ?? 0;
        public int LiczbaNieoplaconych => DzisiejszeRezerwacje?.Count(r => !r.CzyOplacona) ?? 0;

        public AdminDashboardView(IRezerwacjaService service)
        {
            InitializeComponent();
            _service = service;
            DzisiejszeRezerwacje = new ObservableCollection<Rezerwacja>();
            DataContext = this;
            ZaladujDane();
        }

        private async void ZaladujDane()
        {
            try
            {
                // Pobierz rezerwacje z pełnymi danymi (Include Kort, Uzytkownik, StatusRezerwacji)
                var dane = await _service.PobierzWszystkieRezerwacjeZDatyAsync(DateTime.Today);

                DzisiejszeRezerwacje.Clear();

                // Sortuj po godzinie rozpoczęcia
                var posortowane = dane
                    .Where(r => r.StatusRezerwacjiId != 3) // Pomijamy anulowane
                    .OrderBy(r => r.DataRezerwacji);

                foreach (var r in posortowane)
                {
                    DzisiejszeRezerwacje.Add(r);
                }

                // Powiadom UI, że statystyki się zmieniły
                OnPropertyChanged(nameof(LiczbaOplaconych));
                OnPropertyChanged(nameof(LiczbaNieoplaconych));
                OnPropertyChanged(nameof(DzisiejszeRezerwacje));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd ładowania danych: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}