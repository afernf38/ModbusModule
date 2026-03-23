using System;
using System.Windows.Forms;

namespace ModbusTester
{
    /// <summary>
    /// Punto de entrada principal de la aplicacion WinForms.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal de la aplicacion.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmModbusTester());
        }
    }
}
