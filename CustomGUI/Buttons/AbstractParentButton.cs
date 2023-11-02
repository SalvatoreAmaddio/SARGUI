using SARGUI.Converters;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace SARGUI.CustomGUI
{
    public abstract class AbstractParentButton : Button
    {
        private Image Image { get; set; } = new();

        #region ButtonImageSource
        public static readonly DependencyProperty ButtonImageSourceProperty
        = View.Binder.Register<ImageSource?, AbstractParentButton>(nameof(ButtonImageSource), true, null, null, true, true, true);
        public ImageSource? ButtonImageSource
        {
            get => (ImageSource?)GetValue(ButtonImageSourceProperty);
            set => SetValue(ButtonImageSourceProperty, value);
        }
        #endregion

        #region SelfDataContext
        public static readonly DependencyProperty SelfDataContextProperty
        = View.Binder.Register<bool, AbstractParentButton>(nameof(SelfDataContext), true, false, SelfDataContextPropertyChanged, true, true, true);

        private static void SelfDataContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue!=null && (bool)e.NewValue)
            {
                BindingOperations.SetBinding(d, CommandParameterProperty,
                                   new Binding(nameof(DataContext))
                                   {
                                       RelativeSource = RelativeSource.Self,
                                       BindsDirectlyToSource = true
                                   });
                return;
            }
         BindingOperations.ClearBinding(d, CommandParameterProperty);
        }

        public bool SelfDataContext
        {
            get => (bool)GetValue(SelfDataContextProperty);
            set => SetValue(SelfDataContextProperty, value);
        }
        #endregion

        protected AbstractParentButton()
        {
            Padding = new(0);
            Margin = new(0);
            Height = 30;
            AddChild(Image);
            View.Binder.BindUp(this, nameof(ButtonImageSource), Image, Image.SourceProperty);
        }

        protected AbstractParentButton(string tooltip, string key) : this()
        {
            ToolTip = tooltip;
            SelfDataContext = true;
            ButtonImageSource = View.GetResource<ImageSource>(key);
        }
    }
    
    public class ImageButton : AbstractParentButton
    {
        public ImageButton() : base()
        {
        }
    }

    public class ClearButton : AbstractParentButton
    {
        public ClearButton() : base("Clear", "ResetIcon")
        {
        }
    }

    public class NewRecordButton : AbstractParentButton
    {
        public NewRecordButton() : base("Add New Record", "AddNew")
        {
            Background = Brushes.Transparent;
            BorderThickness = new(0);
        }
    }

    public class OpenButton : AbstractParentButton
    {
        public OpenButton() : base("Open Record", "OpenFolderIcon")
        {
            SetBinding(Button.CommandProperty, new Binding("DataContext.OpenCMD")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor,typeof(IView), 1)
            });
        }
    }

    public class DeleteButton : AbstractParentButton
    {
        public DeleteButton() : base("Delete Record", "DeleteIcon")
        {
            SetBinding(Button.CommandProperty, new Binding("DataContext.DeleteCMD")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(IView), 1)
            });
        }
    }

    public class SaveButton : AbstractParentButton
    {
        public SaveButton() : base("Save Record","SaveIcon")
        {
            SetBinding(Button.CommandProperty, new Binding("DataContext.SaveCMD")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(IView), 1)
            });

        }
    }

    public class ExcelButton : AbstractParentButton
    {
        public ExcelButton() : base("Excel", "ExcelIcon")
        {
        }
    }
}