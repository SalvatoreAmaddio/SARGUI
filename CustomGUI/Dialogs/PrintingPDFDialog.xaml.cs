using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SARGUI.CustomGUI {

    public partial class PrintingPDFDialog : AbstractDialog
    {
        bool progressCompleted = false;
        Func<ProgressBar, Task> ProgressLogic;

        public PrintingPDFDialog(Func<ProgressBar, Task> logic)
        {
            ProgressLogic = logic;
            InitializeComponent();
            Closing += async (sender, e) =>
            {
                if (!progressCompleted) await ShowThenHideProgressText();
                e.Cancel = !progressCompleted;
            };
        }

        public async Task ShowMe()
        {
            Show();
            await ProgressLogic(Pb);
            progressCompleted = true;
            Close();
        }

        async Task ShowThenHideProgressText()
        {
            ProgressText.Visibility = Visibility.Visible;
            await Task.Delay(1000);
            ProgressText.Visibility = Visibility.Hidden;
        }
    }
}