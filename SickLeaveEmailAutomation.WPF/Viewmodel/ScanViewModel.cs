using Microsoft.Extensions.Configuration;
using SickLeaveEmailAutomation.WPF.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WIA;

namespace SickLeaveEmailAutomation.WPF.ViewModel
{
    public class ScanViewModel : ViewModelBase
    {
        private readonly IConfiguration _configuration;
        private GmailService _gmailService;

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

        public ICommand ScanCommand { get; set; }
        public ICommand OpenImageCommand { get; set; }
        public ICommand SendEmailCommand { get; set; }

        public ScanViewModel(IConfiguration configuration)
        {
            _configuration = configuration;

            var email = _configuration["Gmail:MyEmail"];
            var appPassword = _configuration["Gmail:AppPassword"];
            _gmailService = new GmailService(email, appPassword);

            SendEmailCommand = new RelayCommand(async (param) => await SendEmailAsync(), (param) => CanSendEmail());
            ScanModel = new ScanModel();
            ScanCommand = new RelayCommand(async (param) => await ScanAsync());
            OpenImageCommand = new RelayCommand(OpenImage);
        }

        private bool CanSendEmail()
        {
            return !string.IsNullOrEmpty(ScanModel.ImagePath) && File.Exists(ScanModel.ImagePath);
        }

        private async Task SendEmailAsync()
        {
            try
            {
                string senderName = _configuration["Gmail:MyName"];
                string senderEmail = _configuration["Gmail:Email"];
                string recipientEmail = _configuration["Gmail:RecipientEmail"];
                string subject = _configuration["Gmail:Subject"];
                string body = BuildEmailBody();

                SetEmailSendingButtonIsEnabled(false);
                await _gmailService.SendEmailAsync(senderName, recipientEmail, subject, body, ScanModel.ImagePath);
                MessageBox.Show("Email sent successfully!");
                await Task.Delay(1000);
                SetEmailSendingButtonIsEnabled(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while sending the email: {ex.Message}");
                SetEmailSendingButtonIsEnabled(true);
            }
        }

        private string BuildEmailBody()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Kedves Címzett!\n");
            sb.Append('\n');
            sb.Append("Csatolva küldöm a tárgyban megjelölt dokumentumot.\n");
            sb.Append('\n');
            sb.Append("Köszönettel:\n");
            sb.Append(_configuration["Gmail:Myname"]);

            return sb.ToString();
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
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressMessage));
            });

            try
            {
                await Task.Run(async () =>
                {
                    ((IProgress<int>)progress).Report(10);
                    await Task.Delay(500);

                    CommonDialog dialog = new CommonDialog();
                    Device device = dialog.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, true, false);
                    if (device != null)
                    {
                        Item item = device.Items[1];

                        ((IProgress<int>)progress).Report(30);
                        await Task.Delay(1000);

                        ImageFile imageFile = (ImageFile)item.Transfer("{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}");

                        ((IProgress<int>)progress).Report(70);
                        await Task.Delay(500);

                        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string targetFolder = Path.Combine(documentsPath, "Keresokeptlelen igazolasok");

                        if (!Directory.Exists(targetFolder))
                        {
                            Directory.CreateDirectory(targetFolder);
                        }

                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        string filename = string.Concat(_configuration["Gmail:AttachmentName"], timestamp, ".jpg");
                        string fullPath = Path.Combine(targetFolder, filename);

                        imageFile.SaveFile(fullPath);
                        ScanModel.ImagePath = fullPath;

                        ((IProgress<int>)progress).Report(100);

                        OnPropertyChanged(nameof(ScanModel));
                        OnPropertyChanged(nameof(ScanModel.ImagePath));
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("No scanner device found. Please ensure the scanner is connected and turned on.", "Scanner Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                        ((IProgress<int>)progress).Report(0);
                    }
                });
            }
            catch (COMException comEx)
            {
                MessageBox.Show($"A COM exception occurred: {comEx.Message} (Error Code: {comEx.ErrorCode:X})", "Scanner Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ProgressMessage = "Error occurred during scanning.";
                Progress = 0;
                SetScanningButtonIsEnabled(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Scanner Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ProgressMessage = "Error occurred during scanning.";
                Progress = 0;
                SetScanningButtonIsEnabled(true);
            }
            SetScanningButtonIsEnabled(true);
        }

        void SetScanningButtonIsEnabled(bool isEnabled)
        {
            IsScanningButtonEnabled = isEnabled;
            OnPropertyChanged(nameof(IsScanningButtonEnabled));
        }

        void SetEmailSendingButtonIsEnabled(bool isEnabled)
        {
            IsEmailSendingButtonEnabled = isEnabled;
            OnPropertyChanged(nameof(IsEmailSendingButtonEnabled));
        }

        private void OpenImage(object parameter)
        {
            if (!string.IsNullOrEmpty(ScanModel.ImagePath) && File.Exists(ScanModel.ImagePath))
            {
                Process.Start(new ProcessStartInfo(ScanModel.ImagePath) { UseShellExecute = true });
            }
        }
    }
}