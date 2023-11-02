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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SARGUI.CustomGUI
{
    public partial class DropDownButton : Button
    {
        private bool DropDownIsOpen = true;
        private string open = View.GetResource<string>("Up");
        private string closed = View.GetResource<string>("Down");

        #region DropDownMaxHeight
        public static readonly DependencyProperty DropDownMaxHeightProperty
        = View.Binder.Register<double, DropDownButton>(nameof(DropDownMaxHeight), true, 100, null, true, true, true);

        public double DropDownMaxHeight
        {
            get => (double)GetValue(DropDownMaxHeightProperty);
            set => SetValue(DropDownMaxHeightProperty, value);
        }
        #endregion

        #region DropDownMinHeight
        public static readonly DependencyProperty DropDownMinHeightProperty
        = View.Binder.Register<double, DropDownButton>(nameof(DropDownMinHeight), true, 30, null, true, true, true);

        public double DropDownMinHeight
        {
            get => (double)GetValue(DropDownMinHeightProperty);
            set => SetValue(DropDownMinHeightProperty, value);
        }
        #endregion

        #region DropDownProperty
        public static readonly DependencyProperty DropDownProperty
        = View.Binder.Register<FrameworkElement, DropDownButton>(nameof(DropDown), false, null, null, true, true, true);

        public FrameworkElement DropDown
        {
            get => (FrameworkElement)GetValue(DropDownProperty);
            set => SetValue(DropDownProperty, value);
        }
        #endregion

        private DoubleAnimation CreateAnimation()
        {
            DoubleAnimation animation = new()
            {
                Duration = new(new TimeSpan(0, 0, 0, 0, 250)),
                EasingFunction = new ExponentialEase()
            };
            return animation;
        }

        public DropDownButton()
        {
            InitializeComponent();
        }
        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            if (DropDown==null) 
            {
                MessageBox.Show("DropDown Property has not been set");
                return;
            }

            DoubleAnimation heightAnimation = CreateAnimation();
            if (DropDownIsOpen) 
            {
                DropDownIsOpen = false;
                Content = closed;
                heightAnimation.To = DropDownMinHeight;
            } 
            else 
            {
                DropDownIsOpen = true;
                Content = open;
                heightAnimation.To = DropDownMaxHeight;
            }

            DropDown.BeginAnimation(HeightProperty, heightAnimation);
        }
    }
}
