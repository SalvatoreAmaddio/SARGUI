using System.ComponentModel;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;

namespace SARGUI.CustomGUI
{
    public abstract class AbstractDialog : Window, IPersonalDialog
    {
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public void MoveCursorAt(int x, int y) => SetCursorPos(x, y);
        public DialogResponse Response { get; set; } = DialogResponse.OK;

        public AbstractDialog()
        {
            SystemSounds.Beep.Play();
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Response.Equals(DialogResponse.VOID))
            {
                Response = DialogResponse.NO;
            }
            base.OnClosing(e);
        }
    }

    public enum DialogResponse
    {
        VOID,
        YES,
        NO,
        OK,
    }

    public interface IPersonalDialog
    {
        public void MoveCursorAt(int x, int y);
        public DialogResponse Response { get; set; }
    }
}
