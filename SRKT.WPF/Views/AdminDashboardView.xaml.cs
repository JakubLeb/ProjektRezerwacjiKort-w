using SRKT.Business.Services;
using SRKT.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace SRKT.WPF.Views
{
    public partial class AdminDashboardView : UserControl
    {
        private readonly IRezerwacjaService _service;
        public ObservableCollection<Rezerwacja> DzisiejszeRezerwacje { get; set; }
        public DateTime DzisiejszaData { get; set; } = DateTime.Now;

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
        }
    }
}