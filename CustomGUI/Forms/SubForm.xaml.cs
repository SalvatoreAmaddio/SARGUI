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
    public partial class SubForm : Grid
    {
        public SubForm()
        {
            InitializeComponent();
        }

        #region ParentControllerProperty
        public static readonly DependencyProperty ParentControllerProperty
        = View.Binder.Register<IAbstractController, SubForm>(nameof(ParentController), true, null, null, true, true, true);

        public IAbstractController ParentController
        {
            get => (IAbstractController)GetValue(ParentControllerProperty);
            set => SetValue(ParentControllerProperty, value);
        }
        #endregion

    }
}
