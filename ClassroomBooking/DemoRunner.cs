using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace ClassroomBooking
{
    // Режим для автоматической съёмки скриншотов: ClassroomBooking.exe --shot файл.png --steps "1,301,0,10:00,12:00;..."
    // (пользователь, аудитория, смещение даты в днях, начало, конец). Для обычного запуска не используется.
    internal static class DemoRunner
    {
        [DllImport("user32.dll")] static extern bool SetProcessDPIAware();

        public static void Run(MainWindow w)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "--shot");
            if (i < 0) return;
            string file = args[i + 1];
            int j = Array.IndexOf(args, "--steps");
            var log = new List<string>();
            if (j >= 0)
            {
                foreach (var step in args[j + 1].Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var p = step.Split(',');
                    w.SetInput(int.Parse(p[0]), p[1], DateTime.Today.AddDays(int.Parse(p[2])), p[3], p[4]);
                    w.Click();
                    log.Add($"{step} => {w.Status} (всего бронирований: {w.BookingsCount})");
                }
            }
            w.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(700) };
                t.Tick += (s, e) =>
                {
                    t.Stop();
                    ScreenShot.Save(w, file);
                    File.WriteAllLines(file + ".txt", log);
                    Application.Current.Shutdown();
                };
                t.Start();
            }));
        }
    }

    internal static class ScreenShot
    {
        public static void Save(Window w, string file)
        {
            var src = PresentationSource.FromVisual(w);
            double sx = src.CompositionTarget.TransformToDevice.M11, sy = src.CompositionTarget.TransformToDevice.M22;
            int x = (int)(w.Left * sx), y = (int)(w.Top * sy), wd = (int)(w.ActualWidth * sx), ht = (int)(w.ActualHeight * sy);
            using var bmp = new Bitmap(wd + 16, ht + 16);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(x - 8, y - 8, 0, 0, bmp.Size);
            bmp.Save(file, ImageFormat.Png);
        }
    }
}


