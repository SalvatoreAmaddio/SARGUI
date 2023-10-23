using System.Windows;

namespace SARGUI.CustomGUI
{
    public partial class ActionConfirmedDialog : AbstractDialog
    {
        public ActionConfirmedDialog(string message = "",string buttonText="THANKS",string title="Changes Applied")
        {
            InitializeComponent();
            Title= title;
            if (message.Length>0) MessageText.Text = message;
            OkButton.Content=buttonText;

        }

        private void CloseClick(object sender, RoutedEventArgs e) => Close();
    }
}
