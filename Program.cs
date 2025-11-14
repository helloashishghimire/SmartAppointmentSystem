using System;
using System.Windows.Forms;
using SmartAppointments.App.Data;
using SmartAppointments.App.Forms;

namespace SmartAppointments.App
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                ApplicationConfiguration.Initialize();
                // This is where it usually fails if PostgreSQL is not reachable
                DbInitializer.EnsureDatabase();
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                // Show any hidden error
                MessageBox.Show(ex.ToString(), "Startup error");
            }
        }
    }
}
