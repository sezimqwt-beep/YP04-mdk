using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HelpDesk.Helpers
{
    // Вспомогательный класс для автоматической съёмки скриншотов:
    // HelpDesk.exe --shot файл.png --login admin --pass 12345 --target MainWindow|msgbox
    // В обычной работе программы не используется.
    public static class DemoRunner
    {
        [DllImport("user32.dll")] static extern IntPtr FindWindow(string cls, string title);
        [DllImport("user32.dll")] static extern bool PostMessage(IntPtr h, uint msg, IntPtr w, IntPtr l);

        static string Arg(string name)
        {
            var a = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(a, name);
            return i >= 0 && i + 1 < a.Length ? a[i + 1] : null;
        }

        public static void Run(Window w)
        {
            string file = Arg("--shot");
            if (file == null) return;
            string login = Arg("--login"), pass = Arg("--pass"), target = Arg("--target") ?? "LoginWindow";
            if (login != null)
            {
                ((TextBox)w.FindName("LoginTextBox")).Text = login;
                ((PasswordBox)w.FindName("PasswordInput")).Password = pass ?? "";
            }
            if (target == "msgbox")
            {
                var bd = new Window { WindowStyle = WindowStyle.None, WindowState = WindowState.Maximized, ShowInTaskbar = false, Background = System.Windows.Media.Brushes.Gainsboro };
                bd.Show();
                w.Activate();
            }
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1200) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                if (target == "msgbox") Capture(Center(w), file);
                else
                {
                    Window t = w;
                    foreach (Window x in Application.Current.Windows) if (x.GetType().Name == target) t = x;
                    Capture(Bounds(t), file);
                }
                IntPtr mb = FindWindow("#32770", null);
                if (mb != IntPtr.Zero) PostMessage(mb, 0x10, IntPtr.Zero, IntPtr.Zero);
                Application.Current.Shutdown();
            };
            timer.Start();
            if (login != null)
            {
                var btn = (Button)w.FindName("LoginButton");
                w.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                    btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent))));
            }
        }

        static Rectangle Bounds(Window w)
        {
            var src = PresentationSource.FromVisual(w);
            double sx = src.CompositionTarget.TransformToDevice.M11, sy = src.CompositionTarget.TransformToDevice.M22;
            return new Rectangle((int)(w.Left * sx), (int)(w.Top * sy), (int)(w.ActualWidth * sx), (int)(w.ActualHeight * sy));
        }

        static Rectangle Center(Window w)
        {
            var b = Bounds(w);
            int cx = b.X + b.Width / 2, cy = b.Y + b.Height / 2;
            return new Rectangle(cx - 420, cy - 230, 840, 460);
        }

        static void Capture(Rectangle r, string file)
        {
            using var bmp = new Bitmap(r.Width, r.Height);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(r.X, r.Y, 0, 0, bmp.Size);
            bmp.Save(file, ImageFormat.Png);
        }
    }
}

