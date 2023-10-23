using SARGUI;
using System.Windows;
using System.Windows.Controls;

namespace SARGUI.CustomGUI
{
    /// <summary>
    /// Interaction logic for CommandPanel.xaml
    /// </summary>
    public partial class CommandPanel : Border
    {
        Grid MainGrid = new();
        Label HeaderLabel;
        
        public CommandPanel()
        {
            InitializeComponent();
            HeaderLabel = new() 
            { 
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold,
            };
            MainGrid.RowDefinitions.Add(new() { Height = new(30) });
            MainGrid.RowDefinitions.Add(new() { Height = new(1,GridUnitType.Star)});
            MainGrid.Children.Add(HeaderLabel);
            Grid.SetRow(HeaderLabel, 0);
            View.Binder.BindUp(this, nameof(Header), HeaderLabel, Label.ContentProperty);
            Header = "COMMANDS";
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (visualAdded is StackPanel panel)
            {
                Child = MainGrid;
                panel.Margin = new(5);
                ContentPanel = panel;
                MainGrid.Children.Add(ContentPanel);
                Grid.SetRow(ContentPanel, 1);
                return;
            }
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }


        #region HeaderProperty
        public static readonly DependencyProperty HeaderProperty
        = View.Binder.Register<string, CommandPanel>(nameof(Header), true, string.Empty);

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
        #endregion

        #region ContentPanelProperty
        public static readonly DependencyProperty ContentPanelProperty
        = View.Binder.Register<StackPanel, CommandPanel>(nameof(ContentPanel), true, null);

        public StackPanel ContentPanel
        {
            get => (StackPanel)GetValue(ContentPanelProperty);
            set => SetValue(ContentPanelProperty, value);
        }
        #endregion
    }

}
