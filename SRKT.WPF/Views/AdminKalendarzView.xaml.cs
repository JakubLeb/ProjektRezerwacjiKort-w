using SRKT.WPF.ViewModels;
using System.Windows.Controls;

namespace SRKT.WPF.Views
{
    public partial class AdminKalendarzView : UserControl
    {
        public AdminKalendarzView()
        {
            InitializeComponent();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is AdminKalendarzViewModel viewModel && viewModel.OdswiezKalendarzCommand.CanExecute(null))
            {
                viewModel.OdswiezKalendarzCommand.Execute(null);
            }
        }
    }
}