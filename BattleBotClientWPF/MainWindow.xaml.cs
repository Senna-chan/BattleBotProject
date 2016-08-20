using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using BattleBotClientWPF.Helpers;
using BattleBotClientWPF.Properties;
using BattleBotClientWPF.ViewModels;
using BattleBotClientWPF.Views;

namespace BattleBotClientWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private DamageSimulator damageSimulator = new DamageSimulator();

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            CloseApp();
        }

        private void MainWindow_Closing(object sender, RoutedEventArgs e)
        {
            CloseApp();
        }

        public static void CloseAppS(bool normal = false)
        {
            if(normal)
                VariableStorage._socketHelper.SendToServer("CLIENT:DISCONNECT");
            App.ControllerHandler.RequestStop();
            Application.Current.Shutdown(0);
            Thread.Sleep(1000);
            Process.GetCurrentProcess().Kill();
        }
        public void CloseApp(bool normal = false)
        {
            if(normal)
                VariableStorage._socketHelper.SendToServer("CLIENT:DISCONNECT");
            App.ControllerHandler.RequestStop();
            Application.Current.Shutdown(0);
            Thread.Sleep(1000);
            Process.GetCurrentProcess().Kill();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            var pressedKey = e.Key;
            if (pressedKey == Key.NumPad1)
            {
                damageSimulator.SimulateDamage(DamageSimulator.DamageTypes.WebCam, 75);
                //webBrowser.Reload();
            }
            if (pressedKey == Key.NumPad0)
            {
                damageSimulator.Repair();
                //webBrowser.Reload();
            }
        }


        private void ChangeHealthVisibility(object sender, RoutedEventArgs e)
        {
            VariableStorage.ViewModel.isHealthVisible = !VariableStorage.ViewModel.isHealthVisible;
            Settings.Default.IsHealthVisible = VariableStorage.ViewModel.isHealthVisible;
            Settings.Default.Save();
        }

        private void ChangeArmorVisibility(object sender, RoutedEventArgs e)
        {
            VariableStorage.ViewModel.isArmorVisible = !VariableStorage.ViewModel.isArmorVisible;
            Settings.Default.IsArmorVisible = VariableStorage.ViewModel.isArmorVisible;
            Settings.Default.Save();
        }

        private void ChangeAmmoVisibility(object sender, RoutedEventArgs e)
        {
            VariableStorage.ViewModel.isAmmoVisible = !VariableStorage.ViewModel.isAmmoVisible;
            Settings.Default.IsAmmoVisible = VariableStorage.ViewModel.isAmmoVisible;
            Settings.Default.Save();
        }

        private void ConfigureClipSize(object sender, RoutedEventArgs e)
        {
            var dialog = new TextDialog("Enter Ammount of ammo that a clip can contain", "Configure Clip Size", Settings.Default.ClipSize);
            dialog.ShowDialog();
            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            {
                VariableStorage.ViewModel.ClipSize = dialog.GetEnteredText();
                Settings.Default.ClipSize = dialog.GetEnteredText();
                Settings.Default.Save();
            }
        }

        private void ConfigureAmmoAmmount(object sender, RoutedEventArgs e)
        {
            var dialog = new TextDialog("Enter Ammount of ammo that you have", "Configure Ammo", Settings.Default.AmmoSize);
            dialog.ShowDialog();
            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            {
                VariableStorage.ViewModel.AmmoSize = dialog.GetEnteredText();
                Settings.Default.AmmoSize = dialog.GetEnteredText();
                Settings.Default.Save();
            }
        }

        private void ChangeHealthColor(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPicker();
            dialog.ShowDialog();
            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            {
                VariableStorage.ViewModel.HealthColor = dialog.GetSelectedColor();
                Settings.Default.HealthColor = dialog.GetSelectedColor();
                Settings.Default.Save();
            }
        }

        private void ChangeArmorColor(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPicker();
            dialog.ShowDialog();
            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            {
                VariableStorage.ViewModel.ArmorColor = dialog.GetSelectedColor();
                Settings.Default.ArmorColor = dialog.GetSelectedColor();
                Settings.Default.Save();
            }
        }

        private void ChangeCrosshariColor(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPicker();
            dialog.ShowDialog();
            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            {
                VariableStorage.ViewModel.CrossHairCollor = dialog.GetSelectedColor();
                Settings.Default.CrosshairColor = dialog.GetSelectedColor();
                Settings.Default.Save();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            VariableStorage.ViewModel = ((MainViewModel)base.DataContext);
        }

        private void ChangeControllerConfig(object sender, RoutedEventArgs e)
        {
            string ControllerConfig = ((System.Windows.Controls.MenuItem)sender).Header.ToString();
            if (ControllerConfig == "ArduinoSerial") // Here we need to specify a COM port
            {
                
            }
            VariableStorage.ViewModel.ControllerConfig = ControllerConfig;
            Settings.Default.ControllerConfig = ControllerConfig;
            App.changeControllerHandling();
        }

        private void ReconnectToSocket(object sender, RoutedEventArgs e)
        {
            App.setupSocketConnection();
        }

        private void CloseBattleBotServer(object sender, RoutedEventArgs e)
        {
            VariableStorage._socketHelper.SendToServer("C:exit");
        }
    }
}
