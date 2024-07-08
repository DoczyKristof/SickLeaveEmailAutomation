using Microsoft.Extensions.Configuration;
using SickLeaveEmailAutomation.WPF.Model;
using SickLeaveEmailAutomation.WPF.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SickLeaveEmailAutomation.WPF.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IConfiguration _configuration;
        private readonly EmailSendingService _emailSendingService;
        private readonly FileScanService _fileScanService;

        private ScanModel _scanModel;
        public ScanModel ScanModel
        {
            get { return _scanModel; }
            set { SetProperty(ref _scanModel, value); }
        }

        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set { SetProperty(ref _progress, value); }
        }

        private string _progressMessage;
        public string ProgressMessage
        {
            get { return _progressMessage; }
            set { SetProperty(ref _progressMessage, value); }
        }

        private bool _isEmailSendingButtonEnabled = true;
        public bool IsEmailSendingButtonEnabled
        {
            get { return _isEmailSendingButtonEnabled; }
            set { SetProperty(ref _isEmailSendingButtonEnabled, value); }
        }

        private bool _isScanningButtonEnabled = true;
        public bool IsScanningButtonEnabled
        {
            get { return _isScanningButtonEnabled; }
            set { SetProperty(ref _isScanningButtonEnabled, value); }
        }

        private string _buildNumber;
        public string BuildNumber
        {
            get { return _buildNumber; }
            set { SetProperty(ref _buildNumber, value); }
        }

        public ICommand ScanCommand { get; set; }
        public ICommand OpenImageCommand { get; set; }
        public ICommand SendEmailCommand { get; set; }

        public MainWindowViewModel(IConfiguration configuration, EmailSendingService emailSendingService, FileScanService fileScanService)
        {
            _configuration = configuration;
            _emailSendingService = emailSendingService;
            _fileScanService = fileScanService;

            SendEmailCommand = new RelayCommand(async (param) => await SendEmailAsync(), (param) => CanSendEmail());
            ScanCommand = new RelayCommand(async (param) => await ScanAsync());
            OpenImageCommand = new RelayCommand(OpenImage);

            ScanModel = new ScanModel();
            BuildNumber = GetBuildNumber();
        }

        private string GetBuildNumber()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"Build: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private bool CanSendEmail()
        {
            return !string.IsNullOrEmpty(ScanModel.ImagePath) && File.Exists(ScanModel.ImagePath);
        }

        private async Task SendEmailAsync()
        {
            try
            {
                SetEmailSendingButtonIsEnabled(false);
                await _emailSendingService.SendEmail(ScanModel);
                MessageBox.Show("Email sent successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while sending the email: {ex.Message}");
            }
            finally
            {
                await Task.Delay(1000);
                SetEmailSendingButtonIsEnabled(true);
            }
        }

        private async Task ScanAsync()
        {
            SetScanningButtonIsEnabled(false);
            var progress = new Progress<int>(value =>
            {
                Progress = value;
                switch (value)
                {
                    case 10:
                        ProgressMessage = "Initializing scanner...";
                        break;
                    case 30:
                        ProgressMessage = "Scanning in progress...";
                        break;
                    case 70:
                        ProgressMessage = "Saving scanned image...";
                        break;
                    case 100:
                        ProgressMessage = "Scan complete!";
                        break;
                }
            });

            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string targetFolder = Path.Combine(documentsPath, "Keresokeptlelen igazolasok");
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string filename = $"{_configuration["Gmail:AttachmentName"]}{timestamp}.jpg";

                string filePath = await Task.Run(() => _fileScanService.ScanAsync(targetFolder, filename, progress));
                if (filePath != null)
                {
                    ScanModel.ImagePath = filePath;
                    OnPropertyChanged(nameof(ScanModel));
                    OnPropertyChanged(nameof(ScanModel.ImagePath));
                }
                else
                {
                    ProgressMessage = "No scanner device has been chosen or is available.";
                    Progress = 0;
                }
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"A COM exception occurred: {comEx.Message} (Error Code: {comEx.ErrorCode:X})", "Scanner Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ProgressMessage = "Error occurred during scanning.";
                Progress = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Scanner Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ProgressMessage = "Error occurred during scanning.";
                Progress = 0;
            }
            finally
            {
                SetScanningButtonIsEnabled(true);
            }
        }

        private void OpenImage(object parameter)
        {
            if (!string.IsNullOrEmpty(ScanModel.ImagePath) && File.Exists(ScanModel.ImagePath))
            {
                Process.Start(new ProcessStartInfo(ScanModel.ImagePath) { UseShellExecute = true });
            }
        }

        private void SetScanningButtonIsEnabled(bool isEnabled)
        {
            IsScanningButtonEnabled = isEnabled;
        }

        private void SetEmailSendingButtonIsEnabled(bool isEnabled)
        {
            IsEmailSendingButtonEnabled = isEnabled;
        }
    }
}