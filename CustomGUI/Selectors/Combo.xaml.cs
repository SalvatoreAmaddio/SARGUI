using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SARGUI.Converters;
using System.Threading.Tasks;
using SARModel;
using System.Linq;

namespace SARGUI.CustomGUI
{
    public partial class Combo : ComboBox
    {
        public TrafficLighterEvent TrafficLighterEvent { get; set; } = new();
        public Combo()
        {
            InitializeComponent();
            View.Binder.BindUp(this, nameof(FilterDataContext), TrafficLighterEvent, TrafficLighterEvent.FilterDataContextFlagProperty, BindingMode.TwoWay, new PropertyToTriggerEventConverter());
            View.Binder.BindUp(this, nameof(RecordsOrganizer), TrafficLighterEvent, TrafficLighterEvent.AbstractListRestructurerFlagProperty, BindingMode.TwoWay, new PropertyToTriggerEventConverter());
            TrafficLighterEvent.GreenLight += OnGreenLight;
            View.Binder.BindUp(this, nameof(SelectedItem), this, ShowPlaceHolderProperty, BindingMode.TwoWay, new ObjectToVisibilityConverter());
        }

        private async void OnGreenLight(object? sender, TrafficLighterEventArgs e) 
        {
            if (RecordsOrganizer == null) return;
            if (!e.IsGreen()) return;
            await Dispatcher.InvokeAsync(SetOrganisedSourceAsync);
        }
        
        Task SetOrganisedSourceAsync()
        {
            if (RecordsOrganizer == null) return Task.CompletedTask;
            RecordsOrganizer?.SetDataContext(FilterDataContext);
            RecordsOrganizer?.Run();
            ItemsSource = RecordsOrganizer?.GetOrganisedSource();

            if (RecordsOrganizer?.SelectedItem!=null)
            {
                SelectedItem = RecordsOrganizer.SelectedItem;
            }

            Dispatcher.BeginInvoke(() =>
                {
                    if (Parent is not null and FrameworkElement frmwrkElmnt)
                    {
                        if (frmwrkElmnt.DataContext is not null && frmwrkElmnt.DataContext is IAbstractModel model)
                            model.IsDirty = false;
                    }
                });
            return Task.CompletedTask;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (RecordsOrganizer == null) return;
            var removedCount=e.RemovedItems?.Count;
            var addedCount = e.AddedItems?.Count;

            if (addedCount>0 && RecordsOrganizer.SelectedItem!=null)
            {
                if (RecordsOrganizer.SelectedItem.Equals(e.AddedItems[0]))
                {
                    e.Handled= true;
                    return;
                } 
                else 
                {
                    RecordsOrganizer.SelectedItem = e.AddedItems[0];
                    e.Handled = true;
                }
            }

            if (removedCount>0) RecordsOrganizer.SelectedItem= (IAbstractModel?)e?.RemovedItems[0];            
        }

        #region FilterDataContext
        public static readonly DependencyProperty FilterDataContextProperty= View.Binder.Register<object?, Combo>(nameof(FilterDataContext), true, string.Empty, null, true, true, true);

        public object? FilterDataContext
        {
            get => (object?)GetValue(FilterDataContextProperty);
            set => SetValue(FilterDataContextProperty, value);
        }
        #endregion

        #region RecordsOrganizerProperty
        public static readonly DependencyProperty RecordsOrganizerProperty
        = View.Binder.Register<AbstractRecordsOrganizer?, Combo>(nameof(RecordsOrganizer), true, null, (d,e)=> ((Combo)d).SetRecordsOrganizerRequeryEvent((IRecordsOrganizer)e.NewValue));

        public AbstractRecordsOrganizer? RecordsOrganizer
        {
            get => (AbstractRecordsOrganizer?)GetValue(RecordsOrganizerProperty);
            set => SetValue(RecordsOrganizerProperty, value);
        }

        private void SetRecordsOrganizerRequeryEvent(IRecordsOrganizer recordsOrganizer)=> 
        recordsOrganizer.OnRequery += RecordsOrganizerOnRequery;
        
        private async void RecordsOrganizerOnRequery(object? sender, RequeryEventArgs e)=>
        await Dispatcher.InvokeAsync(SetOrganisedSourceAsync);

        #endregion

        #region TextInputManagerProperty
        public static readonly DependencyProperty TextInputManagerProperty
        = View.Binder.Register<TextInputManager, Combo>(nameof(TextInputManager), true, null);

        public TextInputManager TextInputManager
        {
            get => (TextInputManager)GetValue(TextInputManagerProperty);
            set => SetValue(TextInputManagerProperty, value);
        }
        #endregion

        #region PlaceHolderProperty
        public static readonly DependencyProperty PlaceHolderProperty
        = View.Binder.Register<string, Combo>(nameof(PlaceHolder), true, string.Empty);

        public string PlaceHolder
        {
            get => (string)GetValue(PlaceHolderProperty);
            set => SetValue(PlaceHolderProperty, value);
        }
        #endregion

        #region ShowPlaceHolder
        public static readonly DependencyProperty ShowPlaceHolderProperty
        = View.Binder.Register<Visibility, Combo>(nameof(ShowPlaceHolder), true, Visibility.Visible);

        public Visibility ShowPlaceHolder
        {
            get => (Visibility)GetValue(ShowPlaceHolderProperty);
            set => SetValue(ShowPlaceHolderProperty, value);
        }
        #endregion
        
    }


}