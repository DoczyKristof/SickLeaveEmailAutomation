using Microsoft.Extensions.Configuration;
using SickLeaveEmailAutomation.WPF.ViewModel;
using System.Windows;

namespace SickLeaveEmailAutomation.WPF.View
{
    public partial class MainWindow : Window
    {
        private readonly IConfiguration _configuration;
        private readonly ScanViewModel _scanViewModel;

        public MainWindow(IConfiguration configuration, ScanViewModel scanViewModel)
        {
            _configuration = configuration;
            _scanViewModel = scanViewModel;
            DataContext = _scanViewModel;
            InitializeComponent();
        }
    }
}