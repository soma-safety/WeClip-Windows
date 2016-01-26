using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Safety.WeClip
{
    public class ClipboardMonitor : IDisposable
    {
        private const int WM_DRAWCLIPBOARD = 0x0308; // WM_DRAWCLIPBOARD
        private const int WM_CHANGECBCHAIN = 0x030D; // WM_CHANGECBCHAIN
        private const int WM_DESTROY = 0x0002; // WM_DESTROY

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetClipboardSequenceNumber();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, Int32 lParam);

        private IntPtr m_clipboardViewerNext = IntPtr.Zero;
        private readonly IntPtr m_handle = IntPtr.Zero;

        public event Action<ClipDataType, object> OnClipboardChanged = null;

        public ClipboardMonitor(IntPtr handle)
        {
            m_handle = handle;
            m_clipboardViewerNext = IntPtr.Zero;

            m_clipboardViewerNext = SetClipboardViewer(m_handle);
        }

        public void WndProc(ref Message message)
        {
            if (message.Msg == WM_DRAWCLIPBOARD)
            {
                uint value = GetClipboardSequenceNumber();
                Console.WriteLine(value.ToString());

                if (OnClipboardChanged != null)
                {
                    
                    IDataObject clipboardData = Clipboard.GetDataObject();

                    if (clipboardData.GetDataPresent(DataFormats.Text) == true)
                    {
                        OnClipboardChanged(ClipDataType.Text, clipboardData.GetData(DataFormats.Text));
                    }
                }

                SendMessage(m_clipboardViewerNext, (uint)message.Msg, message.WParam.ToInt32(), message.LParam.ToInt32());
            }
            else if (message.Msg == WM_CHANGECBCHAIN)
            {
                if (message.WParam == m_clipboardViewerNext)
                    m_clipboardViewerNext = message.LParam;
                else if (m_clipboardViewerNext != IntPtr.Zero)
                    SendMessage(m_clipboardViewerNext, (uint) message.Msg, message.WParam.ToInt32(), message.LParam.ToInt32());
            }
            else if (message.Msg == WM_DESTROY)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            if(m_handle != IntPtr.Zero)
                ChangeClipboardChain(m_handle, m_clipboardViewerNext);
        }
    }
}
