﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace SARGUI.CustomGUI
{
    public partial class LoginForm : Grid
    {
        Window ParentWindow;
        bool isPwdVisible = false;
        string UserName = string.Empty;
        string Password = string.Empty;
        bool RememberMe = false;
        readonly Window NextWindow;
        bool _isMissingUserName => UserName.Length == 0;
        bool _isMissingPassword => Password.Length == 0;
        int _loginAttempts = 0;
        readonly int maxAttempts = 3;
        int _leftAttempts => maxAttempts - _loginAttempts;
        readonly Func<string,string,bool> CheckCredentials;
        readonly Action<string, string> RememberLogedinUser;

        public LoginForm() 
        {
            InitializeComponent();
        }

        public LoginForm(Window nextWindow,Func<string, string, bool> checkCredentials, Action<string, string> rememberLogedinUser) : this()
        {
            NextWindow=nextWindow;
            CheckCredentials += checkCredentials;
            RememberLogedinUser = rememberLogedinUser;
        }

        void FailedLogin()
        {
            FailedAttempts.Visibility = Visibility.Visible;
            IncorrectCredentials.Visibility = Visibility.Visible;
            IncorrectCredentials.Content = "(Username or Password are incorrect)";
            FailedAttempts.Content = (_leftAttempts > 1) ? $"{_leftAttempts} out of {maxAttempts} attempts left!"
                                                      : "This is the last chance.";
        }

        Window GetParentWindow()
        {
            bool found = false;
            FrameworkElement baseObject = this;

            while (!found)
            {
                var parent = baseObject.Parent;
                bool IsWindow = parent is Window;

                if (IsWindow) return (Window)parent;
                
                baseObject = (FrameworkElement)parent;
            }
            throw new Exception("Failed to find Window Parent");
        }

        void LogInClicked(object sender, RoutedEventArgs e)
        {
            ParentWindow ??= GetParentWindow();
            _loginAttempts++;

            if (_loginAttempts == maxAttempts)
            {
                ErrorDialog errorDialog = new("Too many attempts.","Login failed");
                errorDialog.ShowDialog();
                ParentWindow.Close();
                return;
            }

            UserName = UserNameTextBox.Text;
            Password = (isPwdVisible) ? UnmaskedPwdTextBox.Text : PwdTextBox.Password;
            RememberMe = RememberMeCheckBox.IsChecked.Value;
            MissingPasswordLabel.Visibility = (_isMissingPassword) ? Visibility.Visible : Visibility.Hidden;
            MissingUserNameLabel.Visibility = (_isMissingUserName) ? Visibility.Visible : Visibility.Hidden;

            if (_isMissingPassword || _isMissingUserName)
            {
                FailedLogin();
                return;
            }

            bool result = CheckCredentials(UserName, Password);

            if (!result)
            {
                FailedLogin();
                return;
            }
            else
            {
                if (RememberMe) RememberLogedinUser(UserName, Password);
                ParentWindow.Hide();
                NextWindow.Show();
                ParentWindow.Close();
            }
        }

        void ShowPasswordClicked(object sender, RoutedEventArgs e)
        {
            if (!isPwdVisible)
            {
                PwdTextBox.Visibility = Visibility.Collapsed;
                UnmaskedPwdTextBox.Visibility = Visibility.Visible;
                UnmaskedPwdTextBox.Text = PwdTextBox.Password;
                isPwdVisible = true;
                UnmaskedPwdTextBox.Focus();
                return;
            }

            PwdTextBox.Visibility = Visibility.Visible;
            UnmaskedPwdTextBox.Visibility = Visibility.Collapsed;
            PwdTextBox.Password = UnmaskedPwdTextBox.Text;
            isPwdVisible = false;
            PwdTextBox.Focus();
        }
    }

    public class ControlTemplateWrapperForPasswordBox : Border
    {
        PasswordBox pwd;
        Label Placeholder;
        RoundButton ClearButton;

        public ControlTemplateWrapperForPasswordBox()=>Loaded += X_Loaded;
        
        void X_Loaded(object sender, RoutedEventArgs e)
        {
            pwd = (PasswordBox)TemplatedParent;
            Placeholder = (Label)((Grid)Child).Children[1];
            ClearButton = (RoundButton)((Grid)Child).Children[2];
            ClearButton.Click += (sender, e) =>
            {
                pwd.Password = string.Empty;
            };

            pwd.PasswordChanged += (sender, e) =>
            {
                if (pwd.Password.Length > 0)
                {
                    Placeholder.Visibility = Visibility.Hidden;
                    ClearButton.Visibility = Visibility.Visible;
                }
                else
                {
                    Placeholder.Visibility = Visibility.Visible;
                    ClearButton.Visibility = Visibility.Hidden;
                }
            };
            pwd.LostFocus += ManageFocus;
            pwd.GotFocus += ManageFocus;
            pwd.PreviewGotKeyboardFocus += (sender, e) =>
            {
                if (e.NewFocus != null && e.NewFocus is Button)
                {
                    pwd.Password = string.Empty;
                }
            };
        }

        void ManageFocus(object sender, RoutedEventArgs e)
        {
            Visibility buttonVisibility = Visibility.Hidden;
            if (pwd.IsFocused && pwd.Password.Length > 0)
                buttonVisibility = Visibility.Visible;
            ClearButton.Visibility = buttonVisibility;
        }
    }
}