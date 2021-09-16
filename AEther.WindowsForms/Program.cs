
using System;
using System.Windows.Forms;

namespace AEther.WindowsForms
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using var form = new MainForm();

            form.Show();
            while (!form.IsDisposed)
            {
                form.InvokeIfRequired(form.Render);
                Application.DoEvents();
            }

        }
    }
}
