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

namespace SARGUI.CustomGUI
{
    public partial class CurtainContent : Border
    {

        #region SoftwareName
        public static readonly DependencyProperty SoftwareNameProperty
        = View.Binder.Register<string, CurtainContent>(nameof(SoftwareName), true, string.Empty, null, true, true, true);
        
        public string SoftwareName 
        {
            get => (string)GetValue(SoftwareNameProperty);
            set => SetValue(SoftwareNameProperty,value);
        }
        #endregion

        #region Version
        public static readonly DependencyProperty VersionProperty
        = View.Binder.Register<string, CurtainContent>(nameof(Version), true, "1.0.0.", null, true, true, true);
    
        public string Version
        {
            get => (string)GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }
        #endregion

        #region Year
        public static readonly DependencyProperty YearProperty
        = View.Binder.Register<int, CurtainContent>(nameof(Year), true, DateTime.Today.Year, null, true, true, true);

        public int Year
        {
            get => (int)GetValue(YearProperty);
            set => SetValue(YearProperty, value);
        }
        #endregion

        #region IsDemo
        public static readonly DependencyProperty IsDemoProperty
        = View.Binder.Register<bool, CurtainContent>(nameof(IsDemo), true, true, null, true, true, true);

        public bool IsDemo
        {
            get => (bool)GetValue(IsDemoProperty);
            set => SetValue(IsDemoProperty, value);
        }

        public string IsDemoString
        {
            get => (IsDemo) ? "Demo" : "Release";
        }
        #endregion

        #region ClientName
        public static readonly DependencyProperty ClientNameProperty
        = View.Binder.Register<string, CurtainContent>(nameof(ClientName), true, string.Empty, null, true, true, true);


        public string ClientName
        {
            get => (string)GetValue(ClientNameProperty);
            set => SetValue(ClientNameProperty, value);
        }
        #endregion

        public CurtainContent()
        {
            InitializeComponent();
            View.Binder.BindUp(this,nameof(SoftwareName),SoftwareNameLabel,Label.ContentProperty,BindingMode.OneWay);
            View.Binder.BindUp(this, nameof(Version), VersionLabel, Label.ContentProperty, BindingMode.OneWay);
            View.Binder.BindUp(this, nameof(Year), YearLabel, Label.ContentProperty, BindingMode.OneWay);
            View.Binder.BindUp(this, nameof(ClientName), ClientNameLabel, Label.ContentProperty, BindingMode.OneWay);
            View.Binder.BindUp(this, nameof(IsDemoString), IsDemoLabel, Label.ContentProperty, BindingMode.OneWay);
        }
    }
}
