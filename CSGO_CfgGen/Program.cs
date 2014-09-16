using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CSGO_CfgGen
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ConfigFileManager cfgFileMan = new ConfigFileManager();
            MainForm form = new MainForm(cfgFileMan);
            Application.Run(form);
        }
    }
}
