using System.Windows;

namespace SARGUI.CustomGUI
{
    public partial class YesNoDialog : AbstractDialog
    {
        public YesNoDialog(string message = "Do you want to save this record?", string title = "Confirm")
        {
            InitializeComponent();
            Title = title;
            MessageText.Text = message;
        }

        private void YesResponseClicked(object sender, RoutedEventArgs e)
        {
            Response = DialogResponse.YES;
            Close();
        }

        private void NoResponseClicked(object sender, RoutedEventArgs e)
        {
            Response = DialogResponse.NO;
            Close();
        }
    }
}
