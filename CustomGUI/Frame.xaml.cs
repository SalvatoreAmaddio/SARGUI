using SARGUI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SARGUI.CustomGUI
{
    public partial class Frame : Border
    {
        public Frame()
        {
            InitializeComponent();
            View.Binder.BindUp(this, nameof(ImageSource), Picture, Image.SourceProperty);
        }

        public static readonly DependencyProperty ImageSourceProperty
        = View.Binder.Register<ImageSource, Frame>(nameof(ImageSource), true, null,null,true,true,true);

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

    }
}
