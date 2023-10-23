using System;
using SARGUI.Converters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SARGUI.CustomGUI
{
    public partial class DateBox : DatePicker
    {
        SearchBox? PART_TextBox;

        public DateBox() => InitializeComponent();

        #region DateProperty
        public static readonly DependencyProperty DateProperty
        = View.Binder.Register<DateTime?, DateBox>(nameof(Date), true, null);

        public DateTime? Date
        {
            get => (DateTime?)GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }


        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PART_TextBox = (SearchBox)Template.FindName(nameof(PART_TextBox), this);
            View.Binder.BindUp(this, nameof(Date), this, DateBox.SelectedDateProperty, BindingMode.TwoWay,null,DateTime.Today);
            View.Binder.BindUp(this, nameof(Date), PART_TextBox, SearchBox.TextProperty, BindingMode.TwoWay, new DateToStringConverter(this));
        }
    }
}