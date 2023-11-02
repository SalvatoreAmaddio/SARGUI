using SARModel;
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
    public partial class SubForm : Grid, IView
    {
        #region ControllerProperty
        public static readonly DependencyProperty ControllerProperty
        = View.Binder.Register<IAbstractController, SubForm>(nameof(Controller), false, null, null, true, true, true);

        public IAbstractController Controller
        {
            get => (IAbstractController)GetValue(ControllerProperty);
            set => SetValue(ControllerProperty, value);
        }
        #endregion

        public SubForm()
        {
            InitializeComponent();
            View.Binder.BindUp(this, nameof(Controller), this, DataContextProperty,BindingMode.OneWay);
        }

    }
}
