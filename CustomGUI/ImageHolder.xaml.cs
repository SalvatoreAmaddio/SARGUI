using System;
using System.Collections.Generic;
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
using static SARGUI.View;

namespace SARGUI.CustomGUI
{
    /// <summary>
    /// Interaction logic for ImageHolder.xaml
    /// </summary>
    public partial class ImageHolder : Border
    {
        public ImageHolder()
        {
            InitializeComponent();
            Binder.BindUp(this, nameof(PlaceHolderFontSize), TitleLabel, Label.FontSizeProperty);
        }

        #region ImageStorageManager
        public static readonly DependencyProperty ImageStorageManagerProperty = Binder.Register<ImageStorageManager, ImageHolder>(nameof(ImageStorageManager), true, null, null, true, true, true);

        public ImageStorageManager ImageStorageManager
        {
            get => (ImageStorageManager)GetValue(ImageStorageManagerProperty);
            set => SetValue(ImageStorageManagerProperty, value);
        }
        #endregion

        #region PlaceHolderFontSizeProperty
        public static readonly DependencyProperty PlaceHolderFontSizeProperty = Binder.Register<double, ImageHolder>(nameof(PlaceHolderFontSize), true, 20, (d, e) => ((ImageHolder)d).BindUps(e.NewValue), true, true, true);

        public double PlaceHolderFontSize
        {
            get => (double)GetValue(PlaceHolderFontSizeProperty);
            set => SetValue(PlaceHolderFontSizeProperty, value);
        }
        #endregion

        #region PlaceHolderPropertyProperty
        public static readonly DependencyProperty PlaceHolderProperty = Binder.Register<TextBlock, ImageHolder>(nameof(PlaceHolder), true, null, (d, e) => ((ImageHolder)d).BindUps(e.NewValue), true, true, true);

        public TextBlock PlaceHolder
        {
            get => (TextBlock)GetValue(PlaceHolderProperty);
            set => SetValue(PlaceHolderProperty, value);
        }
        #endregion

        void BindUps(object value)=>
        TitleLabel.Content=value;
    }
}
