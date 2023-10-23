using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using SARModel;
using SARGUI.Converters;

namespace SARGUI.CustomGUI
{
    public class FormView : Grid
    {
        protected RecordTracker Tracker=new();
        protected ProgressBar ProgressBar =new() { IsIndeterminate=true};
        protected FormContent FormContent =new();
        protected FormHeader FormHeader =new();
        protected NoWifiStackPanel stack = new();
        protected RecordStatusButton RecordStatusButton = new();
        private readonly ColumnDefinition columnRecordStatusButton = new() { Width = new(20) };
        private readonly ColumnDefinition columnContent = new() { Width = new(1, GridUnitType.Star) };
        private readonly RowDefinition rowHeader = new() { Height = new(0) };
        private readonly RowDefinition rowContent = new() { Height = new(1, GridUnitType.Star) };
        private readonly RowDefinition rowProgressBar = new () { Height = new(10) };
        private readonly RowDefinition rowTracker = new () { Height = new(30)};

        public FormView()
        {
            ColumnDefinitions.Add(columnRecordStatusButton);//0
            ColumnDefinitions.Add(columnContent);//1
            
            RowDefinitions.Add(rowHeader);//0
            RowDefinitions.Add(rowContent);//1
            RowDefinitions.Add(rowProgressBar);//2
            RowDefinitions.Add(rowTracker);//3

            Children.Add(RecordStatusButton);

            SetRow(RecordStatusButton, 1);
            SetColumn(RecordStatusButton, 0);

            Children.Add(ProgressBar);
            SetRow(ProgressBar, 2);
            SetColumn(ProgressBar, 0);
            SetColumnSpan(ProgressBar, 2);
                
            Children.Add(Tracker);
            SetRow(Tracker, 3);
            SetRowSpan(Tracker, 2);
            SetColumnSpan(Tracker, 2);
            
            Children.Add(stack);
            SetColumn(stack, 1);
            SetRow(stack, 3);
        
            View.Binder.BindUp(this, nameof(HeaderHeight), rowHeader, RowDefinition.HeightProperty);
            View.Binder.BindUp(this, nameof(ShowRecordStatusButton), columnRecordStatusButton, ColumnDefinition.WidthProperty,BindingMode.TwoWay, new BoolToGridLengthConverter(20));
            View.Binder.BindUp(this, nameof(ShowProgressBar), rowProgressBar, RowDefinition.HeightProperty, BindingMode.TwoWay, new BoolToGridLengthConverter(10));
            View.Binder.BindUp(this, nameof(ShowRecordTracker), rowTracker, RowDefinition.HeightProperty, BindingMode.TwoWay, new BoolToGridLengthConverter(30));
            View.Binder.BindUp(this, nameof(AllowNewRecord), Tracker, RecordTracker.AllowNewRecordProperty);
            DataContextChanged += FormView_DataContextChanged;
        }

        private void FormView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            View.Binder.BindUp(this, nameof(DataContext), FormContent, FormContent.DataContextProperty);
            View.Binder.BindUp(this, nameof(DataContext), FormHeader,FormHeader.DataContextProperty);

            object? datacontext = FormContent.DataContext;
            if (datacontext is IAbstractController)
            {
                IAbstractController? abstractController = (IAbstractController)((FrameworkElement)FormContent).DataContext;
                View.Binder.BindUp(abstractController, "SelectedRecord", this, ModelProperty);
                View.Binder.BindUp(abstractController, "ChildSource", Tracker, RecordTracker.RecordSourceProperty, BindingMode.OneWay);
                View.Binder.BindUp(abstractController, "IsLoading", ProgressBar, ProgressBar.IsIndeterminateProperty, BindingMode.OneWay);
                return;
            }            
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded is FormContent content)
            {
                FormContent = content;
                SetRow(FormContent, 1);
                SetColumn(FormContent, 1);
                object? datacontext = ((FrameworkElement)FormContent).DataContext;
                if (datacontext is IAbstractController)
                {
                    IAbstractController? abstractController = (IAbstractController)((FrameworkElement)FormContent).DataContext;
                    View.Binder.BindUp(abstractController, "SelectedRecord", this, ModelProperty);
                    View.Binder.BindUp(abstractController, "ChildSource", Tracker, RecordTracker.RecordSourceProperty, BindingMode.OneWay);
                    View.Binder.BindUp(abstractController, "IsLoading", ProgressBar, ProgressBar.IsIndeterminateProperty, BindingMode.OneWay);
                    return;
                } 
            }


            if (visualAdded is FormHeader header)
            {
                FormHeader = header;
                SetRow(FormHeader, 0);
                SetColumn(FormHeader, 0);
                SetColumnSpan(FormHeader, 2);
            }
        }

        #region AllowNewRecord
        public static readonly DependencyProperty AllowNewRecordProperty
        = View.Binder.Register<bool, FormView>(nameof(AllowNewRecord), true, true, null, true, true, true);

        public bool AllowNewRecord
        {
            get => (bool)GetValue(AllowNewRecordProperty);
            set => SetValue(AllowNewRecordProperty, value);
        }
        #endregion

        #region HeaderHeight
        public static readonly DependencyProperty HeaderHeightProperty
        = View.Binder.Register<GridLength, FormView>(nameof(HeaderHeight), true, GridLength.Auto, null,true,true,true);

        public GridLength HeaderHeight
        {
            get => (GridLength)GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }
        #endregion

        #region ShowRecordStatusButton
        public static readonly DependencyProperty ShowRecordStatusButtonProperty
        = View.Binder.Register<bool, FormView>(nameof(ShowRecordStatusButton), true, true, null,true,true,true);

        public bool ShowRecordStatusButton
        {
            get => (bool)GetValue(ShowRecordStatusButtonProperty);
            set => SetValue(ShowRecordStatusButtonProperty, value);
        }
        #endregion

        #region ShowProgressBar
        public static readonly DependencyProperty ShowProgressBarProperty
        = View.Binder.Register<bool, FormView>(nameof(ShowProgressBar), true, true, null,true,true,true);

        public bool ShowProgressBar
        {
            get => (bool)GetValue(ShowProgressBarProperty);
            set => SetValue(ShowProgressBarProperty, value);
        }
        #endregion

        #region ShowRecordTracker
        public static readonly DependencyProperty ShowRecordTrackerProperty
        = View.Binder.Register<bool, FormView>(nameof(ShowRecordTracker), true, true, null,true,true,true);

        public bool ShowRecordTracker
        {
            get => (bool)GetValue(ShowRecordTrackerProperty);
            set => SetValue(ShowRecordTrackerProperty, value);
        }
        #endregion

        #region ModelProperty
        public static readonly DependencyProperty ModelProperty
        = View.Binder.Register<IAbstractModel?, FormView>(nameof(Model), true, null,
        (d,e) => View.Binder.BindUp(e.NewValue, "IsDirty", ((FormView)d).RecordStatusButton, RecordStatusButton.IsDirtyProperty)
        ,true,true,true);

        public IAbstractModel? Model
        {
            get => (IAbstractModel?)GetValue(ModelProperty);
            set 
            {
                SetValue(ModelProperty, value);
            }
        }
        #endregion

    }

    public class FormContent : Border
    {

    }

    public class FormHeader : Border
    {
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            if (visualAdded is Panel)
                View.Binder.BindUp(visualAdded, "Background", this, BackgroundProperty);
        }
    }
}