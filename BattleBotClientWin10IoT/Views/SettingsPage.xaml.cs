using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BattleBotClientWin10IoT.Helpers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BattleBotClientWin10IoT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            this.DataContext = VariableStorage.ViewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
