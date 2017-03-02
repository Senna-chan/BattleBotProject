using System;
using System.Diagnostics;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using BattleBotClientWin10IoT.Helpers;
using BattleBotClientWin10IoT.Models;
using BattleBotClientWin10IoT.ViewModels;
using MjpegProcessor;

namespace BattleBotClientWin10IoT.Views
{
    public partial class MainPage : Page
    {
        
        public MainPage()
        {
            this.InitializeComponent();
            DataContext = VariableStorage.ViewModel;
            if (VariableStorage.BattlebotCameraAddress != null)
            {
                VariableStorage.MjpegDecoder.FrameReady += mjpeg_FrameReady;
                VariableStorage.MjpegDecoder.Error += mjpeg_OnError;
                VariableStorage.MjpegDecoder.ParseStream(
                    new Uri($"http://{VariableStorage.BattlebotCameraAddress.ToString()}:8080/stream/video.mjpeg"));
            }
            VariableStorage.JoyStick.PollController();
            VariableStorage.BattleBotCommunication.StartCommunicating();
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