using SARModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SARGUI.CustomGUI
{
    public partial class RecordTracker : StackPanel
    {
        public RecordTracker()
        {
            InitializeComponent();
            GoNew.Click += GoNewClicked;
            GoNext.Click += GoNextClicked;
            GoPrevious.Click += GoPreviousClicked;
            GoFirst.Click += GoFirstClicked;
            GoLast.Click += GoLastClicked;
            View.Binder.BindUp(this, nameof(AllowNewRecord), GoNew, Button.VisibilityProperty, BindingMode.TwoWay, new BooleanToVisibilityConverter());
        }

        #region NewRecordCommand
        public static readonly DependencyProperty NewRecordCommandProperty
        = View.Binder.Register<ICommand, RecordTracker>(nameof(NewRecordCommand), true, null, NewRecordCommandPropertyChanged, true, true, true);

        private static void NewRecordCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RecordTracker thisTracker = (RecordTracker)d;
            ICommand command = (ICommand) e.NewValue;
            thisTracker.GoNew.Click -= thisTracker.GoNewClicked;
            thisTracker.GoNew.Command = command;
        }

        public ICommand NewRecordCommand
        {
            private get => (ICommand)GetValue(NewRecordCommandProperty);
            set => SetValue(NewRecordCommandProperty, value);
        }
        #endregion

        #region RecordSourceProperty
        public static readonly DependencyProperty RecordSourceProperty
        = View.Binder.Register<IRecordSource?, RecordTracker>(nameof(RecordSource), false, null, 
        (d,e) => View.Binder.BindUp(e.NewValue, "RecordsPositionDisplayer", ((RecordTracker)d).RecordIndicator, Label.ContentProperty, BindingMode.OneWay), true,true,true);

        public IRecordSource? RecordSource
        {
            private get => (IRecordSource?)GetValue(RecordSourceProperty);
            set => SetValue(RecordSourceProperty, value);
        }
        #endregion

        #region AllowNewRecord
        public static readonly DependencyProperty AllowNewRecordProperty
        = View.Binder.Register<bool, RecordTracker>(nameof(AllowNewRecord), true, true, null, true, true, true);

        public bool AllowNewRecord
        {
            get => (bool)GetValue(AllowNewRecordProperty);
            set => SetValue(AllowNewRecordProperty, value);
        }
        #endregion

        private void GoLastClicked(object sender, RoutedEventArgs e)=>
        RecordSource?.GoLast();

        private void GoFirstClicked(object sender, RoutedEventArgs e)=>
        RecordSource?.GoFirst();

        private void GoPreviousClicked(object sender, RoutedEventArgs e)=>
        RecordSource?.GoPrevious();

        private void GoNextClicked(object sender, RoutedEventArgs e)=>
        RecordSource?.GoNext();

        private void GoNewClicked(object sender, RoutedEventArgs e)=>
        RecordSource?.GoNewRecord();
    }
}