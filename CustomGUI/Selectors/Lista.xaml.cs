using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using System.Text;
using SARModel;
using SARGUI.Converters;
using System.Threading.Tasks;
using Style = System.Windows.Style;

namespace SARGUI.CustomGUI
{
    #region Notes
    //ItemsPresenter? Presenter;
    //var x=ItemContainerGenerator.ContainerFromItem(Items.CurrentItem);
    //ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(x);
    #endregion

    public partial class Lista : ListView
    {
        #region PrivateProps
        RowDefinition HeaderHeightRow { get; set; } = new();
        Grid HeaderGrid { get; } = new();
        SelectionColorBackgroundManager? LastUnselected;
        Brush SelectedColor { get => (Brush)this.FindResource("SelectedColor"); }
        #endregion
        public TrafficLighterEvent TrafficLighterEvent { get; set; } = new();
        public ViewCell? ViewCell;

        public Lista() 
        {
            InitializeComponent();
            HeaderGrid.ColumnDefinitions.Add(new() { Width = new(30) });
            SetValue(headersPropertyKey, new HeaderCollection());
            SARGUI.View.Binder.BindUp(this, nameof(FilterDataContext), TrafficLighterEvent, TrafficLighterEvent.FilterDataContextFlagProperty, BindingMode.TwoWay, new PropertyToTriggerEventConverter());
            SARGUI.View.Binder.BindUp(this, nameof(RecordsOrganizer), TrafficLighterEvent, TrafficLighterEvent.AbstractListRestructurerFlagProperty, BindingMode.TwoWay, new PropertyToTriggerEventConverter());
            TrafficLighterEvent.GreenLight += OnGreenLight;
        }

        private async void OnGreenLight(object? sender, TrafficLighterEventArgs e)
        {
            if (RecordsOrganizer == null) return;
            if (!e.IsGreen()) return;
            await SetOrganisedSourceAsync();
        }

        Task SetOrganisedSourceAsync()
        {
            RecordsOrganizer?.SetDataContext(FilterDataContext);
            RecordsOrganizer?.Run();
            ItemsSource = RecordsOrganizer?.GetOrganisedSource();

            Dispatcher.BeginInvoke(() =>
            {
                if (Parent is not null and FrameworkElement frmwrkElmnt)
                {
                    if (frmwrkElmnt.DataContext is not null && frmwrkElmnt.DataContext is IAbstractModel model)
                        model.IsDirty = false;

                    SelectedItem = ItemsSource?.Cast<IAbstractModel>().FirstOrDefault();
                }
            });

            return Task.CompletedTask;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            HeaderBorder = (Border)Template.FindName(nameof(HeaderBorder), this);
            HeaderBorder.Child = HeaderGrid;
            HeaderHeightRow = (RowDefinition)Template.FindName(nameof(HeaderHeightRow), this);
            ViewCell = ItemTemplate.LoadContent() as ViewCell;
            if (ViewCell is null) throw new Exception("ViewCell cannot be null!");
            SARGUI.View.Binder.BindUp(ViewCell, "RecordStatusColumnGridLength", HeaderGrid.ColumnDefinitions[0], ColumnDefinition.WidthProperty, BindingMode.TwoWay);
            SARGUI.View.Binder.BindUp(this, nameof(HeaderHeight), HeaderHeightRow, RowDefinition.HeightProperty, BindingMode.TwoWay);
        }

        public static ContentPresenter? GetContentPresenter(DependencyObject item) => FindVisualChild<ContentPresenter>(item);
        private static childItem? FindVisualChild<childItem>(DependencyObject? obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject? child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem item)
                {
                    return item;
                }
                else
                {
                    childItem? childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        #region HeaderStyle
        public static readonly DependencyProperty HeaderStyleProperty
        = SARGUI.View.Binder.Register<Style, Lista>(nameof(HeaderStyle), true, null, HeaderStylePropertyChanged, true, true, true);

        private static void HeaderStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((Lista)d).ApplyHeaderStyle((Style)e.NewValue);

        public Style HeaderStyle
        {
            get => (Style)GetValue(HeaderStyleProperty);
            set => SetValue(HeaderStyleProperty, value);
        }

        void ApplyHeaderStyle(Style style)
        {
            foreach (var child in HeaderGrid.Children.OfType<FrameworkElement>())
                child.Style = style;
        }
        #endregion

        #region Header
        private static readonly DependencyPropertyKey headersPropertyKey =
        SARGUI.View.Binder.RegisterKey<HeaderCollection, Lista>(nameof(Header),true,new HeaderCollection(), CollectionChanged);

        public static readonly DependencyProperty HeaderProperty = headersPropertyKey.DependencyProperty;

        public HeaderCollection Header => (HeaderCollection)GetValue(HeaderProperty);

        private static void CollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((Lista)d).InGrid((HeaderCollection)e.NewValue);

        void InGrid(HeaderCollection collection)
        {
            StringBuilder defaultColumnsWidth = new();
            HeaderGrid.Children.Clear();
            
            foreach (var d in collection)
            {
                d.Style = HeaderStyle;
                defaultColumnsWidth.Append("*,");
                HeaderGrid.Children.Add(d);
            }

            if (!string.IsNullOrEmpty(HeaderColumnsWidth)) return;
            var len = defaultColumnsWidth.Length;
            if (len > 0) defaultColumnsWidth = defaultColumnsWidth.Remove(len - 1, 1);

            HeaderColumnsWidth=defaultColumnsWidth.ToString();
        }
        #endregion

        #region HeaderBorder
        public static readonly DependencyProperty HeaderBorderProperty = SARGUI.View.Binder.Register<Border, Lista>(nameof(HeaderBorder), true, null, null, true, true, true);

        public Border HeaderBorder
        {
            get => (Border)GetValue(HeaderBorderProperty);
            private set => SetValue(HeaderBorderProperty, value);
        }
        #endregion

        #region HeaderGridStyle
        public static readonly DependencyProperty HeaderGridStyleProperty
        = SARGUI.View.Binder.Register<Style, Lista>(nameof(HeaderGridStyle), true, null, HeaderGridStylePropertyChanged, true, true, true);

        private static void HeaderGridStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((Lista)d).HeaderGrid.Style = (Style)e.NewValue;

        public Style HeaderGridStyle
        {
            get => (Style)GetValue(HeaderGridStyleProperty);
            set => SetValue(HeaderGridStyleProperty, value);
        }
        #endregion

        #region HeaderBorderStyle
        public static readonly DependencyProperty HeaderBorderStyleProperty
        = SARGUI.View.Binder.Register<Style, Lista>(nameof(HeaderBorderStyle), true, null, HeaderBorderStylePropertyChanged, true, true, true);

        private static void HeaderBorderStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((Lista)d).HeaderBorder.Style = (Style)e.NewValue;

        public Style HeaderBorderStyle
        {
            get => (Style)GetValue(HeaderBorderStyleProperty);
            set => SetValue(HeaderBorderStyleProperty, value);
        }
        #endregion

        #region HeaderColumnsWidth
        public static readonly DependencyProperty HeaderColumnsWidthProperty
        = SARGUI.View.Binder.Register<string, Lista>(nameof(HeaderColumnsWidth), true, string.Empty, HeaderColumnsPropertyChanged, true, true, true);

        private static void HeaderColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((Lista)d).InColumns(e.NewValue.ToString());

        public string HeaderColumnsWidth
        {
            get => (string)GetValue(HeaderColumnsWidthProperty);
            set => SetValue(HeaderColumnsWidthProperty, value);
        }

        void InColumns(string? columns)
        {
            if (string.IsNullOrEmpty(columns)) return;

            var ColumnCount =HeaderGrid.ColumnDefinitions.Count;

            if (ColumnCount>1)
                HeaderGrid.ColumnDefinitions.RemoveRange(1, ColumnCount - 1);

            string[] columnsArray = columns.Split(',');

            foreach (string column in columnsArray)
                HeaderGrid.ColumnDefinitions.Add(Decipher(column));
        }

        private static ColumnDefinition Decipher(string value)
        {
            ColumnDefinition columnDefinition = new();
            GridLength len;

            if (value.Equals("*"))
                len = new(1, GridUnitType.Star);

            if (value.Contains('*') && value.Length > 1)
            {
                int index = value.IndexOf("*");
                value = value.Remove(index, 1);
                len = new(Convert.ToDouble(value), GridUnitType.Star);
            }


            if (double.TryParse(value, out _))
                len = new(Convert.ToDouble(value));

            columnDefinition.Width = len;
            return columnDefinition;
        }
        #endregion

        #region HeaderHeight
        public static readonly DependencyProperty HeaderHeightProperty
        = SARGUI.View.Binder.Register<GridLength, Lista>(nameof(HeaderHeight), true, GridLength.Auto, null,true,true,true);

        public GridLength HeaderHeight
        {
            get => (GridLength)GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }
        #endregion

        #region FilterDataContext
        public static readonly DependencyProperty FilterDataContextProperty
        = SARGUI.View.Binder.Register<object?, Lista>(nameof(FilterDataContext), true, null, null,true,true,true);

        public object? FilterDataContext
        {
            get => (object?)GetValue(FilterDataContextProperty);
            set => SetValue(FilterDataContextProperty, value);
        }
        #endregion

        #region RecordsOrganizerProperty
        public static readonly DependencyProperty RecordsOrganizerProperty = SARGUI.View.Binder.Register<AbstractRecordsOrganizer?, Lista>(nameof(RecordsOrganizer), true, null, (d, e) => ((Lista)d).SetRecordsOrganizerRequeryEvent((IRecordsOrganizer)e.NewValue));
        public AbstractRecordsOrganizer? RecordsOrganizer
        {
            get => (AbstractRecordsOrganizer?)GetValue(RecordsOrganizerProperty);
            set => SetValue(RecordsOrganizerProperty, value);
        }
        private void SetRecordsOrganizerRequeryEvent(IRecordsOrganizer recordsOrganizer) => recordsOrganizer.OnRequery += RecordsOrganizerOnRequery;
        private async void RecordsOrganizerOnRequery(object? sender, RequeryEventArgs e) => await SetOrganisedSourceAsync();
        #endregion

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (e.AddedItems.Count <= 0) return;
            IAbstractModel? model = (IAbstractModel?)e.AddedItems[0];
            if (model == null) return;
            if (model.IsNewRecord)
            {
                ScrollIntoView(Items[^1]);
                return;
            }
            ScrollIntoView(e.AddedItems[0]);
        }

        void OnListViewItemSelected(object sender, RoutedEventArgs e)=>LastUnselected?.ResetOriginalBackground();

        void OnListViewItemUnselected(object sender, RoutedEventArgs e)
        {

            ListViewItem item = (ListViewItem)sender;
//            LastSelected?.ResetOriginalBackground();            
            if (!this.IsKeyboardFocusWithin)
            {
                LastUnselected = new(item);
                item.Background = SelectedColor;
            }
            e.Handled = true;
            return;
        }

    }

    #region SelectionColorBackgroundManager
    public class SelectionColorBackgroundManager
    {
        private readonly ListViewItem LastUnselected;
        private readonly Brush OriginalBackground;

        public SelectionColorBackgroundManager(ListViewItem _lastUnselected)
        {
            LastUnselected= _lastUnselected;
            OriginalBackground= _lastUnselected.Background.Clone();
        }

        public void ResetOriginalBackground()=>LastUnselected.Background=OriginalBackground;
    }
    #endregion

    #region HeaderCollection
    public class HeaderCollection : FreezableCollection<FrameworkElement> { }
    #endregion
}