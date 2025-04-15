using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deliverable_2
{
    public static class GlobalSettings
    {
        private static string connectionString = "Data Source=DESKTOP-GDR7F2R\\SQLEXPRESS;Initial Catalog=Project;Integrated Security=True;";
        public static string GetConnectionString()
        {
            return connectionString;
        }
    }

    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Change the following line to start with the desired form
            Application.Run(new Form1()); // Replace AdminReports(50) with the form you want to start with
        }
    }
}
