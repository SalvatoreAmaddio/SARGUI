using System;
using System.Windows;

namespace SARGUI.CustomGUI
{
    #region ListaEventLighter
    public class TrafficLighterEvent : FrameworkElement
    {
        readonly TrafficLighterEventArgs args = new();
        public event EventHandler<TrafficLighterEventArgs>? GreenLight;
        public void TriggerEvent() => GreenLight?.Invoke(this, args);
        public byte AbstractListRestructurerFlag
        {
            get => (byte)GetValue(AbstractListRestructurerFlagProperty);
            set => SetValue(AbstractListRestructurerFlagProperty, value);
        }

        public byte FilterDataContextFlag
        {
            get => (byte)GetValue(FilterDataContextFlagProperty);
            set => SetValue(FilterDataContextFlagProperty, value);
        }

        public static readonly DependencyProperty AbstractListRestructurerFlagProperty
        = View.Binder.Register<byte, TrafficLighterEvent>(nameof(AbstractListRestructurerFlag), true, 0, FlagPropertyChanged);

        public static readonly DependencyProperty FilterDataContextFlagProperty
        = View.Binder.Register<byte, TrafficLighterEvent>(nameof(FilterDataContextFlag), true, 0, FlagPropertyChanged);

        private static void FlagPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TrafficLighterEvent obj = (TrafficLighterEvent)d;
            byte b = (byte)e.NewValue;
            switch (e.Property.Name)
            {
                case nameof(FilterDataContextFlag):
                    obj.args.FilterDataContextFlag = b;
                    break;
                case nameof(AbstractListRestructurerFlag):
                    obj.args.AbstractListRestructurerFlag = b;
                    break;
            }
            obj.TriggerEvent();
        }
    }
    #endregion

    #region SemaforoEventArgs
    public class TrafficLighterEventArgs : EventArgs
    {
        public byte AbstractListRestructurerFlag { get; set; }
        public byte FilterDataContextFlag { get; set; }
        public bool IsGreen() => AbstractListRestructurerFlag >= 1 && FilterDataContextFlag >= 1;
    }
    #endregion

}
