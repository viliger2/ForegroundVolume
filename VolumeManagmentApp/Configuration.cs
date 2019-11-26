using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VolumeManagmentApp
{
    public partial class Configuration : Form
    {
        const int VOLUME_UP_KEY = 175;
        const int VOLUME_DOWN_KEY = 174;

        public Configuration()
        {
            InitializeComponent();
            KeyboardHook.RegisterHotKey(this.Handle, VOLUME_UP_KEY, 0, (int)Keys.VolumeUp);
            KeyboardHook.RegisterHotKey(this.Handle, VOLUME_DOWN_KEY, 0, (int)Keys.VolumeDown);
        }

        public Configuration(bool startMinimized) : this()
        {
            if (startMinimized)
            {
                this.WindowState = FormWindowState.Minimized;
                notifyIcon1.Visible = true;
                this.Hide();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {
                if(m.WParam.ToInt32() == VOLUME_UP_KEY)
                {
                    Control.VolumeUp();
                }
                else if(m.WParam.ToInt32() == VOLUME_DOWN_KEY)
                {
                    Control.VolumeDown();
                }
            }
           base.WndProc(ref m);
        }

        private void Configuration_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                this.Hide();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            System.Threading.Thread.Sleep(200);
            notifyIcon1.Visible = false;
        }

        private void toolStripMenuShow_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            System.Threading.Thread.Sleep(200);
            notifyIcon1.Visible = false;
        }

        private void toolStripMenuClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
