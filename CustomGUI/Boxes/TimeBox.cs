using SARGUI.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SARGUI.CustomGUI
{
    public class TimeBox : SearchBox
    {

        public TimeBox() 
        {
            PlaceHolder = "HH:MM";
            View.Binder.BindUp(this,nameof(Time),this,TextProperty,BindingMode.TwoWay,new TimeToStringConverter());
        }

        #region TimeProperty
        public static readonly DependencyProperty TimeProperty
        = View.Binder.Register<DateTime?, TimeBox>(nameof(Time), true, null);
        //,StringFormat={}{0:HH:mm}
        public DateTime? Time
        {
            get => (DateTime?)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }
        #endregion
    }
}
