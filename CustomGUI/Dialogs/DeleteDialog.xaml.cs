using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SARGUI.CustomGUI
{
    public partial class DeleteDialog : AbstractDialog
    {
        public DeleteDialog()
        {
            InitializeComponent();
            Response = DialogResponse.VOID;
            Loaded += (sender, e) =>
            {
                Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(delegate ()
                {
                    FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;
                    TraversalRequest request = new(focusDirection);
                    UIElement elementWithFocus = (UIElement)Keyboard.FocusedElement;
                    var relativePoint = NoOption.PointToScreen(new Point(0d, 0d));
                    MoveCursorAt((int)relativePoint.X + 45, (int)relativePoint.Y + 20);
                }));
            };
        }

        private void YesResponseClicked(object sender, RoutedEventArgs e)
        {
            Response = DialogResponse.YES;
            Close();
        }

        private void NoResponseClicked(object sender, RoutedEventArgs e)
        {
            Response = DialogResponse.NO;
            Close();
        }
    }
}
