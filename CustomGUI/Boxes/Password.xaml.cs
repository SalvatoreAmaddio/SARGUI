using SARGUI.Converters;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using static SARGUI.View;

namespace SARGUI.CustomGUI
{
    public partial class Password : Grid
    {
        private readonly Label _placeholder;
        private readonly Button _clearButton;
        public Password()
        {
            InitializeComponent();
            pwd.ApplyTemplate();
            _placeholder = (Label)pwd.Template.FindName(nameof(_placeholder), pwd);
            _clearButton = (Button)pwd.Template.FindName(nameof(_clearButton), pwd);
            View.Binder.BindUp(this, nameof(ShowClearButton),_clearButton,Button.VisibilityProperty);
            View.Binder.BindUp(txt, "PlaceHolder", _placeholder, Label.ContentProperty);
            View.Binder.BindUp(txt, "ShowPlaceHolder", _placeholder, Label.VisibilityProperty);
            View.Binder.BindUp(this, nameof(Value), txt, SearchBox.TextProperty);

            View.Binder.MultiBindUp(
                this,
                ShowClearButtonProperty,
                new StringAndBoolToVisibilityConverter(),
                View.Binder.QuickBindUp(this, nameof(Value)),
                View.Binder.QuickBindUp(pwd, nameof(IsFocused), BindingMode.OneWay)
                );
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            pwd.Password = Value;
        }

        #region Value
        public static readonly DependencyProperty ValueProperty
        = View.Binder.Register<string, Password>(nameof(Value), true, string.Empty);
        
        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);            
        }
        #endregion

        #region ShowClearButtonProperty
        public Visibility ShowClearButton
        {
            get => (Visibility)GetValue(ShowClearButtonProperty);
            set => SetValue(ShowClearButtonProperty, value);
        }

        public static readonly DependencyProperty ShowClearButtonProperty
        = View.Binder.Register<Visibility, Password>(nameof(ShowClearButton), true, Visibility.Hidden);
        #endregion

        private void PasswordChanged(object sender, RoutedEventArgs e) => Value = pwd.Password;
        private void TogglePassoword(object sender, RoutedEventArgs e) 
        {
            pwd.Visibility = (pwd.Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;
            pwd.PasswordChanged-=PasswordChanged;
            pwd.Password = Value;
            pwd.PasswordChanged+=PasswordChanged;
        }
        private void RoundButton_Click(object sender, RoutedEventArgs e) => pwd.Password = string.Empty;
        private void pwd_PreviewGotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus != null && e.NewFocus is Button)
            {
                pwd.Password = string.Empty;
            }
        }
    }
}
