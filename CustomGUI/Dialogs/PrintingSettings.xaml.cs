using SARModel;
using System.Windows;
using System.Windows.Forms;

namespace SARGUI.CustomGUI
{
    public partial class PrintingSettings : Window
    {
        MicrosoftPDFManager microsoftPDFManager;

        public PrintingSettings(ref MicrosoftPDFManager PDFManager) 
        {
            InitializeComponent();
            microsoftPDFManager= PDFManager;
        }

        private void ConfirmClicked(object sender, RoutedEventArgs e)
        {

            if (FileName.Text.Length==0)
            {
                ErrorDialog errorDialog = new("You must specify a file name");
                errorDialog.ShowDialog();
                return;
            }

            if (FileNamePath.Text.Length == 0)
            {
                ErrorDialog errorDialog = new("You must specify a file path");
                errorDialog.ShowDialog();
                return;
            }

            microsoftPDFManager.SetFileName($"{FileNamePath.Text}\\{FileName}.pdf");
            Close();
        }

        private void PickPathButtonClicked(object sender, RoutedEventArgs e)
        {

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select where to save the pdf.";
                dialog.UseDescriptionForTitle = true;
                DialogResult result = dialog.ShowDialog();
                if (result.ToString().Equals("OK"))
                {
                    FileNamePath.Text = dialog.SelectedPath;
                }
            }
        }
    }
}
