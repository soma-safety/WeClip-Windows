using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using Safety.WeClip;

namespace TestWeClipLib
{
    //[TestFixture, RequiresThread(ApartmentState.STA)]
    [TestFixture]
    public class TestClipboardMonitor
    {
        private ClipboardForm m_form;
        private Task m_formTask;
        private Thread m_thread;

        public class ClipboardForm : Form
        {
            public ClipboardMonitor ClipboardMonitor;

            public ClipboardForm(ref bool formInitialized)
            {
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
                Visible = false;

                ClipboardMonitor = new ClipboardMonitor(Handle);
                formInitialized = true;
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                ClipboardMonitor?.WndProc(ref m);
            }

            public void CallInFormThread(Action func)
            {
                Invoke(func);
            }
        }

        private ClipboardMonitor ClipboardMonitor {  get { return m_form.ClipboardMonitor; } }

        [SetUp]
        public void Init()
        {
            bool formInitialized = false;

            m_thread = new Thread((ThreadStart)(() => {
                m_form = new ClipboardForm(ref formInitialized);
                Application.Run(m_form);
            }));

            m_thread.SetApartmentState(ApartmentState.STA);
            m_thread.Start();

            while (formInitialized == false) ;
        }

        [TearDown]
        public void Release()
        {
            if (m_form != null)
            {
                m_form.CallInFormThread(() =>
                {
                    m_form.Close();
                    m_form.Dispose();
                });
                m_form = null;
            }

            m_thread.Join();
        }

        [Test]
        public void TestClipBoardEventNotify()
        {
            bool called = false;
            int callCount = 0;

            ClipboardMonitor.OnClipboardChanged += (type, data) =>
            {
                called = true;
                ++callCount;
            };

            m_form.CallInFormThread(() => Clipboard.SetText("text", TextDataFormat.Text));

            Assert.True(called);
            Assert.AreEqual(2, callCount); // why?
        }

        [Test]
        public void TestClipboardTextChanged()
        {
            string text = "clipboard text";
            ClipDataType resultType = ClipDataType.Invalid;
            object resultData = string.Empty;

            ClipboardMonitor.OnClipboardChanged += (type, data) =>
            {
                resultType = type;
                resultData = data;
            };

            m_form.CallInFormThread(() => Clipboard.SetText(text, TextDataFormat.Text));

            Assert.AreEqual(ClipDataType.Text, resultType);
            Assert.AreEqual(text, resultData);
        }
    }
}
