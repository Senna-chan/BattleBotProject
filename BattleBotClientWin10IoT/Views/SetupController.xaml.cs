using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Gaming.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BattleBotClientWin10IoT.ViewModels;
using System.Threading;
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

        public async void MapInput()
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
            int firstFoundPosition=-1;
            while (true)
            {
                controller.GetCurrentReading(buttons, switches, axis);
                counter = 0;
                switch (setupCount)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        foreach (var axiss in axis)
                        {
                            var roundedaxis = Math.Round(axiss, 1);
                            if (roundedaxis > 0.9 || roundedaxis < 0.1 && roundedaxis != Math.Round(oldaxis[counter],1))
                            {
                                if (firstFoundPosition == counter)
                                {
                                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                        CoreDispatcherPriority.Normal, () =>
                                        {
                                            switch (setupCount)
                                            {
                                                case 0:
                                                    SetupControllerViewModel.SpeedAxis = counter;
                                                    setupCount++;
                                                    firstFoundPosition = -1;
                                                    break;
                                                case 1:
                                                    SetupControllerViewModel.TurnAxis = counter;
                                                    setupCount++;
                                                    firstFoundPosition = -1;
                                                    break;
                                                case 2:
                                                    SetupControllerViewModel.PanAxis = counter;
                                                    setupCount++;
                                                    firstFoundPosition = -1;
                                                    break;
                                                case 3:
                                                    SetupControllerViewModel.TiltAxis = counter;
                                                    setupCount++;
                                                    firstFoundPosition = -1;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        });
                                }
                                firstFoundPosition = counter;
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
