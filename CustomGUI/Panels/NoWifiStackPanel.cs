using SARModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static SARGUI.View;

namespace SARGUI.CustomGUI
{
    public class NoWifiStackPanel : StackPanel
    {
        NoWifiLabel NoWifiLabel = new();

        public NoWifiStackPanel() 
        {
            Orientation = Orientation.Horizontal;
            HorizontalAlignment = HorizontalAlignment.Right;
            VerticalAlignment = VerticalAlignment.Center;
            Children.Add(new NoWifiImage());
            Children.Add(NoWifiLabel);
            this.Visibility = Visibility.Hidden;
            InternetConnection.StatusEvent += OnInternetStatusEvt;
        }

        private void OnInternetStatusEvt(object? sender, InternetConnection.ConnectionCheckerArgs e)
        {            
            Visibility = Convert.ToBoolean(e.IsConnected) ? Visibility.Hidden : Visibility.Visible;
            NoWifiLabel.Content= e.Message;
        }

    }

    public class NoWifiImage : Image
    {
        public NoWifiImage() 
        {
            Source = GetResource<ImageSource>("NoWifi");
            VerticalAlignment = System.Windows.VerticalAlignment.Center;
            Stretch =System.Windows.Media.Stretch.Uniform;
            ToolTip = "Icon by Kyoz";
            Margin = new(0, 0, 5, 0);
        }
    }

    public class NoWifiLabel : Label
    {
        public NoWifiLabel()
        {
            Padding = new(0);
            Foreground = Brushes.Red;
            FontWeight = FontWeights.Bold;
            VerticalAlignment = VerticalAlignment.Center;
            VerticalContentAlignment = VerticalAlignment.Center;
            FontSize = 20;
        }
    }
}
