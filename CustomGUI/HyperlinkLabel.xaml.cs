using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SARGUI.CustomGUI
{
    public partial class HyperlinkLabel : TextBlock
    {
        public HyperlinkLabel()
        {
            InitializeComponent();
        }

        private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessStartInfo info = new(e.Uri.AbsoluteUri);
            info.UseShellExecute = true;
            Process.Start(info);
            e.Handled = true;
        }
    }
}
