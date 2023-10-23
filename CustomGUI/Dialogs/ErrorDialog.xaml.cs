using System.Windows;

namespace SARGUI.CustomGUI
{
    public partial class ErrorDialog : AbstractDialog
    {
        public ErrorDialog(string text,string caption="Input Error")
        {
            InitializeComponent();
            Title = caption;
            Message(text);
        }
        
        public void Message(string text)=>TextMessage.Content = text;
        private void OkClicked(object sender, RoutedEventArgs e) => Close();
    }
}
