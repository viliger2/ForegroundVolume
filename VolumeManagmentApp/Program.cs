using System;
using System.Windows.Forms;

namespace VolumeManagmentApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool startMinimized = true;
            foreach (string arg in args)
            {
                // silent launch, start minimized
                if (arg.Equals("-s"))
                {
                    startMinimized = true;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Configuration(startMinimized));
        }
    }

}