using System;
using System.Windows.Forms;
using Safety.WeClip.Clip;

namespace Safety.WeClip
{
    public partial class WeClipForm : Form
    {
        private readonly ClipboardMonitor m_clipboardMonitor;

        public WeClipForm()
        {
            InitializeComponent();

            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Visible = false;
            NotifyIcon.Visible = true;
            NotifyIcon.ContextMenuStrip = contextMenuStrip1;
            
            m_clipboardMonitor = new ClipboardMonitor(Handle);
            m_clipboardMonitor.OnClipboardChanged += (type, o) => { Console.WriteLine(o); };
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            m_clipboardMonitor?.WndProc(ref m);
        }
    }
}
