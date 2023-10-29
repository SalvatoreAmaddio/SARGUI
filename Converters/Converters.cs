using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using SARGUI.CustomGUI;
using System.Text;
using SARModel;

namespace SARGUI.Converters
{

    public class LessThanZero : IValueConverter
    {
        protected double d;
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            d = (double)value;
            return d < 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return d;
        }
    }

    public class MoreThanZero : LessThanZero
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            d = (double)value;
            return d > 0;
        }
    }

    #region StringToClearButtonVisibilityConverterClass
    public class StringAndBoolToVisibilityConverter : IMultiValueConverter
    {
        object[]? Values;
        string? Text;
        bool IsFocused;
        bool HasText => Text?.Length > 0;
        bool Show => HasText && IsFocused;
        bool Hide => HasText && !IsFocused;

        public Visibility VisibilityResult
        {
            get
            {
                if (Show)
                    return Visibility.Visible;

                if (Hide)
                    return Visibility.Hidden;

                return Visibility.Hidden;
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Values = values;
            Text = values[0].ToString();
            IsFocused = System.Convert.ToBoolean(values[1]);
            return VisibilityResult;
        }

        public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        Values;
    }
    #endregion

    #region StringToPlaceHolderVisibilityConverterClass
    public class StringToPlaceHolderVisibilityConverter : IValueConverter
    {
        string? Text;
        Visibility VisibilityResult => Text?.Length > 0 ? Visibility.Hidden : Visibility.Visible;

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Text = value?.ToString();
            return VisibilityResult;
        }

        public virtual object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Text;
    }
    #endregion

    #region BoolToForegroundConverter
    public class BoolToBrushConverter : IValueConverter
    {
        object? Value;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Value = value;
            bool result = System.Convert.ToBoolean(value);
            return result ? Brushes.RoyalBlue : Brushes.Black;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Value;
    }
    #endregion

    #region BoolToCursorConverter
    public class BoolToCursorConverter : IValueConverter
    {
        object? Value;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Value = value;
            bool result = System.Convert.ToBoolean(value);
            return result ? Cursors.Hand : Cursors.Arrow;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Value;
    }
    #endregion

    #region BoolToVisibilityConverter
    public class BoolToVisibilityConverter : IValueConverter
    {
        readonly byte type;
        public BoolToVisibilityConverter(byte _type)
        {
            type = _type;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = (bool)value;

            if (type==0)
            {
                return val ? Visibility.Visible : Visibility.Hidden;
            }
            else
            {
                return val ? Visibility.Hidden : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility val = (Visibility)value;

            if (type == 0)
            {
                return val.Equals(Visibility.Visible);
            }
            else
            {
                return val.Equals(Visibility.Hidden);
            }
        }
    }
    #endregion

    #region ObjectToVisibilityConverter
    public class ObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                if (string.IsNullOrEmpty(value.ToString()))
                {
                    value = null;
                }
            }
            return value == null ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    #endregion

    #region DateToStringConverter
    public class DateToStringConverter : IValueConverter
    {
        private readonly DateBox dateBox;
        public DateToStringConverter(DateBox dateBox) => this.dateBox = dateBox;
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            return ((DateTime?)value)?.ToString("dd/MM/yyyy");
        }
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? dateString = value?.ToString();
            try
            {
                if (string.IsNullOrEmpty(dateString)) throw new Exception();
                return DateTime.Parse(dateString, new DateFormat());
            }
            catch
            {
                dateBox.Text=string.Empty;
                return null;
            }
        }
    }
    #endregion

    #region TimeToStringConverter
    public class TimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            DateTime time = (DateTime)value;
            return time.ToString("HH:mm");
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? timestring = value?.ToString();
            if (string.IsNullOrEmpty(timestring)) throw new FormatException();
            try
            {
                return DateTime.Parse(timestring);
            }
            catch(FormatException)
            {
                var s= CheckChars(value?.ToString()?.ToCharArray());
                return (string.IsNullOrEmpty(s)) ? null : DateTime.Parse(s);
            }
        }

        static string CheckChars(char[]? chars)
        {
            if (chars == null) return string.Empty;
            string result= Check(chars);
            
            if (result.Contains(':'))
            {
                var index=result.IndexOf(':');
                result=result.Remove(index,1);
                Adjust(ref result);
            } 
            else if (result.Contains('.'))
            {
                var index = result.IndexOf('.');
                result = result.Remove(index, 1);
                Adjust(ref result);
            } else
            {
                Adjust(ref result);
            }

            return result;
        }

        static void Adjust(ref string result,char separator=':')
        {
            if (result.Length == 1 && IsNumeric(result))
            {
                result = result.Insert(1, $"0{separator}00");
            }

            if (result.Length == 2 && IsNumeric(result))
            {
                result = result.Insert(2, $"{separator}00");
            }

            if (result.Length == 3 && IsNumeric(result))
            {
                result = result.Insert(3, $"{separator}");
                result += "0";
            }

            if (result.Length == 4 && IsNumeric(result))
            {
                result=result.Insert(2, $"{separator}");
            }

        }

        private static string Check(char[] chars)
        {
            StringBuilder sb = new();
            foreach (char c in chars)
            {
                char? currentChar = null;
                if (IsNumeric(c))
                {
                    var number=int.Parse(c.ToString());
                    if (number>59 || number<0)
                    {
                        currentChar = '0';
                    } else
                    {
                        currentChar = c;
                    }
                }

                if (c.Equals(':') || c.Equals('.'))
                {
                    currentChar = c;
                }

                if (currentChar!=null)
                sb.Append(currentChar);
            }
            return sb.ToString();   
        }

        static bool IsNumeric(object obj)
        {
            bool result = true;
            string? stringnumber = obj.ToString();
            if (string.IsNullOrEmpty(stringnumber)) throw new Exception();
            try
            {
               int.Parse(stringnumber);
            }
            catch
            {
                result=false;
            }
            return result;
        }
    }
    #endregion

    #region PropertyToTriggerEventConverter
    public class PropertyToTriggerEventConverter : IValueConverter
    {

        byte b = 0;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ++b;
            if (b > 10)
            {
                b = 1;
            }
            return b;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region BoolToGridLengthConverter
    public class BoolToGridLengthConverter : IValueConverter
    {
        readonly double defaultValue;
        object? Value;
        bool Result => System.Convert.ToBoolean(Value);

        public BoolToGridLengthConverter(double _defaultValue) =>
        defaultValue = _defaultValue;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Value = value;
            return (!Result) ? new GridLength(0) : new GridLength(defaultValue);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Value;
    }
    #endregion

    #region GridLengthToBooleanConverter
    public class GridLengthToBooleanConverter : IValueConverter
    {
        object? Value;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Value = value;
            return ((GridLength)value).Value <= 0;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Value;
    }
    #endregion

    #region StringToBrushColor
    public class StringToBrushColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Transparent;
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(value.ToString()));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "#2aeb2a";
            SolidColorBrush color = (SolidColorBrush)value;
            return color.Color.ToString();
        }
    }
    #endregion

    #region ObjectToStringConverter
    public class ObjectToStringConverter : IMultiValueConverter
    {
        object[]? Values;
        string? Text=string.Empty;
        string? Password=string.Empty;
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Values = values;
            Text = values[0]?.ToString();
            Password = values[1]?.ToString();
            return Text;
        }

        public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        Values;
    }
    #endregion


}
