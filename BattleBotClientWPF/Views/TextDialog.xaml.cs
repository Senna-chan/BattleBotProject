using System.Windows;

namespace BattleBotClientWPF.Views
{
    /// <summary>
    /// Interaction logic for TextDialog.xaml
    /// </summary>
    public partial class TextDialog : Window
    {
        private string _inputedText;
        public TextDialog(string helpText, string windowTitle)
        {
            InitializeComponent();
            this.Title = windowTitle;
            this.LabelDiscription.Text = helpText;
        }
        public TextDialog(string helpText, string windowTitle, string textboxText)
        {
            InitializeComponent();
            this.Title = windowTitle;
            this.LabelDiscription.Text = helpText;
            this.TextBox1.Text = textboxText;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public string GetEnteredText()
        {
            return _inputedText;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            _inputedText = TextBox1.Text;
        }
    }
}
