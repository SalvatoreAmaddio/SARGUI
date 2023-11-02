using MvvmHelpers.Commands;
using SARGUI.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static SARGUI.View;

namespace SARGUI.CustomGUI
{
    public partial class SearchBox : TextBox
    {
        public SearchBox()
        {
            InitializeComponent();

            ContextMenu = new CustomContextMenu(new() {
                new(GetResource<ImageSource>("Copy"), new Command(Copy)),//0
                new(GetResource<ImageSource>("Cut"), new Command(Cut)),//1
                new(GetResource<ImageSource>("Paste"), new Command(Paste)),//2
                new(GetResource<ImageSource>("Selection"), new Command(SelectAll)),//3
                new(GetResource<ImageSource>("CopyAll"), new Command(CopyAll)),//4
                new(GetResource<ImageSource>("UndoRes"), new Command(UndoMe)),//5
                new(GetResource<ImageSource>("Redo"), new Command(RedoMe))//6
            });

            Binder.BindUp(this, nameof(Text), this, ShowPlaceHolderProperty, BindingMode.TwoWay, new StringToPlaceHolderVisibilityConverter());
            Binder.BindUp(this, nameof(IsHyperlink), this, ForegroundProperty, BindingMode.TwoWay, new BoolToBrushConverter());
            Binder.BindUp(this, nameof(IsHyperlink), this, CursorProperty, BindingMode.TwoWay, new BoolToCursorConverter());
            
            Binder.MultiBindUp(
                            this,
                            ShowClearButtonProperty,
                            new StringAndBoolToVisibilityConverter(),
                            Binder.QuickBindUp(this,nameof(Text)),
                            Binder.QuickBindUp(this, nameof(IsFocused),BindingMode.OneWay)
                            );
        }

        #region TextInputManager
        public static readonly DependencyProperty TextInputManagerProperty
        = View.Binder.Register<TextInputManager, SearchBox>(nameof(TextInputManager), true, null);
        public TextInputManager TextInputManager
        {
            get => (TextInputManager)GetValue(TextInputManagerProperty);
            set => SetValue(TextInputManagerProperty, value);
        }
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);
            if (TextInputManager == null) return;

            if (!TextInputManager.AllowInput(e.Text))
            {
                e.Handled = true;
                return;
            }
        }
        #endregion

        #region ShowClearButtonProperty
        public Visibility ShowClearButton
        {
            get => (Visibility)GetValue(ShowClearButtonProperty);
            set => SetValue(ShowClearButtonProperty, value);
        }

        public static readonly DependencyProperty ShowClearButtonProperty
        = View.Binder.Register<Visibility, SearchBox>(nameof(ShowClearButton), true, Visibility.Hidden);
        #endregion

        #region ShowPlaceHolder
        public static readonly DependencyProperty ShowPlaceHolderProperty
        = View.Binder.Register<Visibility, SearchBox>(nameof(ShowPlaceHolder), true, Visibility.Visible);

        public Visibility ShowPlaceHolder
        {
            get => (Visibility)GetValue(ShowPlaceHolderProperty);
            set => SetValue(ShowPlaceHolderProperty, value);
        }
        #endregion

        #region IsHyperlink
        public static readonly DependencyProperty IsHyperlinkProperty
        = View.Binder.Register<bool, SearchBox>(nameof(IsHyperlink), true, false);

        public bool IsHyperlink
        {
            get => (bool)GetValue(IsHyperlinkProperty);
            set => SetValue(IsHyperlinkProperty, value);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (!IsHyperlink) return;
            if (Uri.TryCreate(Text, UriKind.Absolute, out Uri? uri))
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = uri.AbsoluteUri,
                    UseShellExecute = true
                });
            }
        }
        #endregion

        #region PlaceHolder
        public static readonly DependencyProperty PlaceHolderProperty
        = View.Binder.Register<string, SearchBox>(nameof(PlaceHolder), true, string.Empty);

        public string PlaceHolder
        {
            get => (string)GetValue(PlaceHolderProperty);
            set => SetValue(PlaceHolderProperty, value);
        }
        #endregion

        #region CommandFunctions
        void RedoMe() => Redo();
        void UndoMe() => Undo();    
        void CopyAll()
        {
            SelectAll();
            Copy();
        }
        #endregion

        #region OnTextChangedAndComboBoxManagement
        protected override void OnTextChanged(TextChangedEventArgs e)
        {           
            base.OnTextChanged(e);
            //if (TemplatedParent is Combo)
            //{
            //    if (!CheckInputInComboBox())
            //        Dispatcher.BeginInvoke(new Action(() =>
            //        {
            //            MessageBox.Show($"The input '{Text}' is not valid.", "INVALID INPUT");
            //            Undo();
            //        }));
            //}
        }

        bool CheckInputInComboBox()
        {
             Combo combo = (Combo)TemplatedParent;
             var list = combo.ItemsSource.Cast<object>().ToList();
             if (list==null || list.Count==0) return true;
             bool result = list.Any(s => s.Equals(combo.SelectedItem));
            if (result) 
            {
            }
            if (combo.SelectedItem == null)
            {
//                Text = string.Empty;
                return true;
            }
            return result;
        }
        #endregion

        #region ClearButtonEventAndFocusManagement        
        private void RoundButtonClicked(object sender, RoutedEventArgs e) => Text = string.Empty;

        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus != null && e.NewFocus is Button) 
            {
                Text = string.Empty;
            }
        }
        #endregion
    }

    #region TextInputManagerClass
    public abstract class TextInputManager
    {
        public abstract bool AllowInput(string text);
    }
    #endregion

    #region PairsClass
    public class ObjectAndCommandPair
    {
        public object? Resource { get; set; }
        public ICommand Command { get; set; }

        public ObjectAndCommandPair(object? resource, ICommand command)
        {
            Resource = resource;
            Command = command;
        }
    }
    #endregion

    #region CustomContextMenuClass
    public class CustomContextMenu : ContextMenu
    {
        public CustomContextMenu(List<ObjectAndCommandPair> ResourceList)
        {
            AddChild(MakeMenuItem("_Copy", (ImageSource?)ResourceList[0]?.Resource, ResourceList[0].Command));
            AddChild(MakeMenuItem("_Cut", (ImageSource?)ResourceList[1]?.Resource, ResourceList[1].Command));
            AddChild(MakeMenuItem("_Paste", (ImageSource?)ResourceList[2]?.Resource, ResourceList[2].Command));
            AddChild(new Separator());            

            AddChild(MakeMenuItem("_Select All", (ImageSource?)ResourceList[3]?.Resource, ResourceList[3].Command));
            AddChild(new Separator());

            AddChild(MakeMenuItem("_Copy All", (ImageSource?)ResourceList[4]?.Resource, ResourceList[4].Command));
            AddChild(new Separator());

            AddChild(MakeMenuItem("_Undo", (ImageSource?)ResourceList[5]?.Resource, ResourceList[5].Command));
            AddChild(MakeMenuItem("_Redo", (ImageSource?)ResourceList[6]?.Resource, ResourceList[6].Command));

        }

        static MenuItem MakeMenuItem(string header,ImageSource? source,ICommand? command)=>
        new()
            {
                Header=header,
                Icon=new Image() { Source=source},
                Command=command,          
            };            
        }
    #endregion

}