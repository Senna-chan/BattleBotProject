using MjpegProcessor;
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Networking.ServiceDiscovery.Dnssd;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using BattleBotClientWin10IoT.Helpers;
using BattleBotClientWin10IoT.ViewModels;
using mDNS;

namespace BattleBotClientWin10IoT
{
    public sealed partial class MainPage : Page
    {
        
        public MainPage()
        {
            this.InitializeComponent();
            VariableStorage.MjpegDecoder.FrameReady += mjpeg_FrameReady;
            VariableStorage.MjpegDecoder.Error += mjpeg_OnError;
            VariableStorage.MjpegDecoder.ParseStream(new Uri("http://192.168.1.132:8080/stream/video.mjpeg"));
            VariableStorage.ViewModel = ((MainViewModel)base.DataContext);
            InitMDNS();
        }



        private async void InitMDNS()
        {
            mDNS.mDNS client = new mDNS.mDNS();
            await client.Init();
            var services = await client.List("_workstation._tcp.local.");
            if (services.Length == 1) // Assume its the camera
            {
                VariableStorage.BattlebotCameraAddress = services[0].Address;
                VariableStorage.ViewModel.mdnsStatus = "Camera found on: "+services[0].HostAddress;
            }
            else
            {
                VariableStorage.ViewModel.mdnsStatus = "Camera not found";
            }
        }
        
        
        private void mjpeg_OnError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine(e.Message);
        }
        private async void mjpeg_FrameReady(object sender, FrameReadyEventArgs e)
        {
            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                await ms.WriteAsync(e.FrameBuffer);
                ms.Seek(0);
                
                var bmp = new BitmapImage();
                await bmp.SetSourceAsync(ms);
                MjpegImage.Source = bmp;
            }
        }

        private void GotoSettings(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }
    }
}