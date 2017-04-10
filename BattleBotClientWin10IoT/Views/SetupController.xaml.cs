using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BattleBotClientWin10IoT.Helpers;
using BattleBotClientWin10IoT.ViewModels;
using System.Diagnostics;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BattleBotClientWin10IoT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SetupController : Page
    {
        private SetupControllerViewModel SetupControllerViewModel { get; set; }
        public SetupController()
        {
            this.InitializeComponent();
            SetupControllerViewModel = new SetupControllerViewModel();
            DataContext = SetupControllerViewModel;
            Task.Delay(1000).Wait();
            new Task(MapInput, CancellationToken.None, TaskCreationOptions.LongRunning).Start();
        }

        public void MapInput()
        {
            var controller = RawGameController.RawGameControllers.First();
            double[] axis = new double[controller.AxisCount];
            bool[] buttons = new bool[controller.ButtonCount];
            GameControllerSwitchPosition[] switches = new GameControllerSwitchPosition[controller.SwitchCount];
            double[] oldaxis = new double[controller.AxisCount];
            bool[] oldbuttons = new bool[controller.ButtonCount];
            GameControllerSwitchPosition[] oldswitches = new GameControllerSwitchPosition[controller.SwitchCount];
            int setupCount = 0;
            int counter = 0;
            while (true)
            {
                controller.GetCurrentReading(buttons, switches, axis);
                counter = 0;
                switch (setupCount)
                {
                    case 0:
                        foreach (var axiss in axis)
                        {
                            var roundedaxis = Math.Round(axiss, 2);
                            if (roundedaxis > 0.9 || roundedaxis < 0.1 && roundedaxis != oldaxis[counter])
                            {
                                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    SetupControllerViewModel.SpeedAxis = counter;
                                });
                            }
                            counter++;
                        }
                        break;
                    case 1:
                        foreach (var axiss in axis)
                        {
                            if (axiss > 0.9 || axiss < 0.1)
                            {
                                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    SetupControllerViewModel.TurnAxis = counter;
                                });
                            }
                            counter++;
                        }
                        break;
                    case 2:
                        foreach (var axiss in axis)
                        {
                            if (axiss > 0.9 || axiss < 0.1)
                            {
                                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    SetupControllerViewModel.PanAxis = counter;
                                });
                            }
                            counter++;
                        }
                        break;
                    case 3:
                        foreach (var axiss in axis)
                        {
                            if (axiss > 0.9 || axiss < 0.1)
                            {
                                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    SetupControllerViewModel.TiltAxis = counter;
                                });
                            }
                            counter++;
                        }
                        break;
                    default:
                        break;
                }
                oldaxis = axis;
                oldbuttons = buttons;
                oldswitches = switches;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
