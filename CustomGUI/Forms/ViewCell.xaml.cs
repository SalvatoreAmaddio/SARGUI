using SARGUI;
using System.Windows;
using System.Windows.Controls;

namespace SARGUI.CustomGUI
{
    public partial class ViewCell : Grid
    {
        public GridLength RecordStatusColumnGridLength { get; set; } = new(20);
        public ColumnDefinition RecordStatusColumn { get; set; }
        ColumnDefinition column2 = new () { Width = new(1, GridUnitType.Star) };
        protected RecordStatusButton RecordStatusButton = new();
        public FrameworkElement? Child { get; private set; }

        public ViewCell()
        {
            InitializeComponent();
            RecordStatusColumn = new() { Width = RecordStatusColumnGridLength };
            ColumnDefinitions.Add(RecordStatusColumn);
            ColumnDefinitions.Add(column2);
            Children.Add(RecordStatusButton);
            SetRow(RecordStatusButton, 0);
            SetColumn(RecordStatusButton, 0);
        }

        public bool IsSuitable()=>Children[1] is Grid;

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            if (visualAdded is RecordStatusButton) return;
            Child = (FrameworkElement)visualAdded;
            SetColumn(Child, 1);
            View.Binder.BindUp(Child.DataContext, "IsDirty", RecordStatusButton, RecordStatusButton.IsDirtyProperty);
        }


    }
}
