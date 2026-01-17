using SRKT.Business.Services;
using SRKT.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel; // Potrzebne do powiadomień
using System.Linq; // Potrzebne do zliczania rezerwacji
using System.Windows.Controls;

namespace SRKT.WPF.Views
{
    public partial class AdminDashboardView : UserControl, INotifyPropertyChanged
    {
        private readonly IRezerwacjaService _service;
        public ObservableCollection<Rezerwacja> DzisiejszeRezerwacje { get; set; }
        public DateTime DzisiejszaData { get; set; } = DateTime.Now;

        // Statystyki obliczane na bieżąco
        public int LiczbaOplaconych => DzisiejszeRezerwacje.Count(r => r.CzyOplacona);
        public int LiczbaNieoplaconych => DzisiejszeRezerwacje.Count(r => !r.CzyOplacona);

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
            var dane = await _service.PobierzWszystkieRezerwacjeZDatyAsync(DateTime.Today);
            DzisiejszeRezerwacje.Clear();
            foreach (var r in dane) DzisiejszeRezerwacje.Add(r);

            // Powiadom UI, że statystyki się zmieniły
            OnPropertyChanged(nameof(LiczbaOplaconych));
            OnPropertyChanged(nameof(LiczbaNieoplaconych));
            OnPropertyChanged(nameof(DzisiejszeRezerwacje));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}