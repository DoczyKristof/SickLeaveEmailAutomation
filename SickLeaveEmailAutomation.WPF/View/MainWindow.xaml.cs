using Microsoft.Extensions.Configuration;
using SickLeaveEmailAutomation.WPF.ViewModel;
using System.Windows;

namespace SickLeaveEmailAutomation.WPF.View
{
    public partial class MainWindow : Window
    {
        private readonly IConfiguration _configuration;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public MainWindow(IConfiguration configuration, MainWindowViewModel mainWindowViewModel)
        {
            _configuration = configuration;
            _mainWindowViewModel = mainWindowViewModel;
            DataContext = _mainWindowViewModel;
            InitializeComponent();
        }
    }
}