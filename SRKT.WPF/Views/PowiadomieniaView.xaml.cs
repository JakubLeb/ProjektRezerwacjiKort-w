using SRKT.Core.Models;
using SRKT.WPF.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace SRKT.WPF.Views
{
    public partial class PowiadomieniaView : UserControl
    {
        public PowiadomieniaView()
        {
            InitializeComponent();
        }

        private void Powiadomienie_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Powiadomienie powiadomienie)
            {
                if (DataContext is PowiadomieniaViewModel viewModel)
                {
                    viewModel.OznaczJakoPrzeczytaneCommand.Execute(powiadomienie);
                }
            }
        }
    }
}
