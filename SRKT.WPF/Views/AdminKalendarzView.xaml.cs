using SRKT.WPF.ViewModels;
using System.Windows.Controls;

namespace SRKT.WPF.Views
{
    public partial class AdminKalendarzView : UserControl
    {
        private bool _isInitialized = false;

        public AdminKalendarzView()
        {
            InitializeComponent();

            // Oznacz jako zainicjalizowany po załadowaniu
            Loaded += (s, e) => _isInitialized = true;
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Nie wykonuj podczas inicjalizacji - zapobiega błędom przy pierwszym ładowaniu
            if (!_isInitialized)
                return;

            if (DataContext is AdminKalendarzViewModel viewModel && viewModel.OdswiezKalendarzCommand.CanExecute(null))
            {
                viewModel.OdswiezKalendarzCommand.Execute(null);
            }
        }
    }
}
