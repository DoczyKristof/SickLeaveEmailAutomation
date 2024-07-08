using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using WIA;

namespace SickLeaveEmailAutomation.WPF.Services
{
    public class FileScanService
    {
        public async Task<string> ScanAsync(string outputFolder, string fileName, IProgress<int> progress)
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

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                string fullPath = Path.Combine(outputFolder, fileName);

                imageFile.SaveFile(fullPath);

                ((IProgress<int>)progress).Report(100);
                return fullPath;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("No scanner device found. Please ensure the scanner is connected and turned on.", "Scanner Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
                ((IProgress<int>)progress).Report(0);
                return null;
            }
        }
    }
}