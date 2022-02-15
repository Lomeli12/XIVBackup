using System;
using GLib;
using Application = Gtk.Application;

namespace XIVBackup {
    class Program {

        [STAThread]
        public static void Main(string[] args) {
            Application.Init();

            var app = new Application("org.XIVBackup.XIVBackup", ApplicationFlags.None);
            app.Register(Cancellable.Current);

            var win = new MainWindow();
            app.AddWindow(win);

            win.Show();
            Application.Run();
        }
    }
}