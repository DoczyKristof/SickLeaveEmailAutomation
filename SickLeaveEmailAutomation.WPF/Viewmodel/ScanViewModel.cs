using SickLeaveEmailAutomation.WPF.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WIA;

namespace SickLeaveEmailAutomation.WPF.ViewModel
{
    public class ScanViewModel : ViewModelBase
    {
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

        public ICommand ScanCommand { get; set; }
        public ICommand OpenImageCommand { get; set; }

        public ScanViewModel()
        {
            ScanModel = new ScanModel();
            ScanCommand = new RelayCommand(async (param) => await ScanAsync());
            OpenImageCommand = new RelayCommand(OpenImage);
        }

        private async Task ScanAsync()
        {
            try
            {
                ProgressMessage = "Initializing scanner...";
                Progress = 10;

                await Task.Delay(500);

                CommonDialog dialog = new CommonDialog();
                Device device = dialog.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, true, false);
                if (device != null)
                {
                    Item item = device.Items[1];

                    ProgressMessage = "Scanning in progress...";
                    Progress = 30;

                    await Task.Delay(1000);

                    ImageFile imageFile = (ImageFile)item.Transfer("{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}");

                    ProgressMessage = "Saving scanned image...";
                    Progress = 70;

                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string targetFolder = Path.Combine(documentsPath, "Keresokeptlelen igazolasok");

                    if (!Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }

                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string filename = $"doczy_laszlo_igazolas_{timestamp}.jpg";
                    string fullPath = Path.Combine(targetFolder, filename);

                    imageFile.SaveFile(fullPath);
                    ScanModel.ImagePath = fullPath;

                    ProgressMessage = "Scan complete!";
                    Progress = 100;

                    OnPropertyChanged(nameof(ScanModel));
                    OnPropertyChanged(nameof(ScanModel.ImagePath));
                }
                else
                {
                    MessageBox.Show("No scanner device found. Please ensure the scanner is connected and turned on.", "Scanner Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ProgressMessage = "No scanner device found.";
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