using SARGUI;
using SARGUI.Converters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SARGUI.CustomGUI
{
    public partial class MandatoryFlag : Label
    {
        public MandatoryFlag()
        {
            InitializeComponent();
            View.Binder.BindUp(this,nameof(Value),this,VisibilityProperty,BindingMode.TwoWay,new ObjectToVisibilityConverter());
        }

        #region ValueProperty
        public static readonly DependencyProperty ValueProperty
        = View.Binder.Register<object?, MandatoryFlag>(nameof(Value), true, null);

        public object? Value
        {
            get => (object?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        #endregion

    }
}
