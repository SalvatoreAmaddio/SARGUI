using SARGUI.Converters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SARGUI.CustomGUI
{
    public partial class RecordStatusButton : Button
    {
        public RecordStatusButton() 
        {
            InitializeComponent();
            View.Binder.BindUp(this, nameof(IsDirty), Next, VisibilityProperty,BindingMode.TwoWay,new BoolToVisibilityConverter(1));
            View.Binder.BindUp(this, nameof(IsDirty), Edit, VisibilityProperty, BindingMode.TwoWay, new BoolToVisibilityConverter(0));
        }

        #region IsDirtyProperty
        public static readonly DependencyProperty IsDirtyProperty
        = View.Binder.Register<bool, RecordStatusButton>(nameof(IsDirty), true, false, null);

        //private static void IsDirtyPropertyPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)=>
        //((RecordStatusButton)d).UpdateContent();

        public bool IsDirty
        {
            get => (bool)GetValue(IsDirtyProperty);
            set => SetValue(IsDirtyProperty, value);
        }
        #endregion
        
        //void UpdateContent()
        //{
        //    if (IsDirty)
        //    {
        //        Edit.Visibility = Visibility.Visible;
        //        Next.Visibility = Visibility.Hidden;
        //    }
        //    else
        //    {
        //        Edit.Visibility = Visibility.Hidden;
        //        Next.Visibility = Visibility.Visible;
        //    }
        //}

    }
}
