using NUnit.Framework;

namespace Safety.WeClip.History
{
    [TestFixture]
    public class TestClipboardHistory
    {
        private ClipboardHistory m_clipboardHistory;

        private readonly string jsonString = @"{ 'history' :
                                                { 'records' : [
                                                                { 'seq_num':103, 'type':'Text', 'value':'abc3', 'device':'pc1' },
                                                                { 'seq_num':102, 'type':'Text', 'value':'abc2', 'device':'pc1' },
                                                                { 'seq_num':101, 'type':'Text', 'value':'abc1', 'device':'pc1' }
                                                              ]
                                                }
                                              }";

        [SetUp]
        public void Init()
        {
            m_clipboardHistory = new ClipboardHistory();
        }

        [TearDown]
        public void Release()
        {
            m_clipboardHistory.Dispose();
        }

        [Test]
        public void TestAddClipData()
        {
            Assert.AreEqual(0, m_clipboardHistory.Count);

            Assert.That(m_clipboardHistory.AddNewClipData(ClipDataType.Text, "abc"), Is.True);
            Assert.AreEqual(1, m_clipboardHistory.Count);

            Assert.That(m_clipboardHistory.AddNewClipData(ClipDataType.Text, "abc2"), Is.True);
            Assert.AreEqual(2, m_clipboardHistory.Count);
        }

        [Test]
        public void TestAddTwiceSameClipData()
        {
            string text = "abc";
            Assert.AreEqual(0, m_clipboardHistory.Count);

            Assert.That(m_clipboardHistory.AddNewClipData(ClipDataType.Text, text), Is.True);
            Assert.AreEqual(1, m_clipboardHistory.Count);

            Assert.That(m_clipboardHistory.AddNewClipData(ClipDataType.Text, text), Is.False);
            Assert.AreEqual(1, m_clipboardHistory.Count);
        }

        [Test]
        public void TestHistoryLimit()
        {
            m_clipboardHistory.SetHistoryLimit(3);
            Assert.AreEqual(0, m_clipboardHistory.Count);

            Assert.That(m_clipboardHistory.AddNewClipData(ClipDataType.Text, "1"), Is.True);
            Assert.AreEqual(1, m_clipboardHistory.Count);

            Assert.That(m_clipboardHistory.AddNewClipData(ClipDataType.Text, "2"), Is.True);
            Assert.AreEqual(2, m_clipboardHistory.Count);

            Assert.That(m_clipboardHistory.AddNewClipData(ClipDataType.Text, "3"), Is.True);
            Assert.AreEqual(3, m_clipboardHistory.Count);

            Assert.That(m_clipboardHistory.AddNewClipData(ClipDataType.Text, "4"), Is.True);
            Assert.AreEqual(3, m_clipboardHistory.Count);

            Assert.That(m_clipboardHistory.CurrentClipboardData, Is.Not.Null);
            Assert.That(m_clipboardHistory.CurrentClipboardData.DataType, Is.EqualTo(ClipDataType.Text));
            Assert.That((string)m_clipboardHistory.CurrentClipboardData.Data, Is.EqualTo("2"));
        }

        [Test]
        public void TestLoadFromJSON()
        {
            Assert.AreEqual(0, m_clipboardHistory.Count);
            m_clipboardHistory.SetHistoryLimit(3);

            Assert.That(m_clipboardHistory.Load(jsonString), Is.True);
            Assert.AreEqual(3, m_clipboardHistory.Count);
        }

        [Test]
        public void TestLoadErrorFromMalformedJSON()
        {
            Assert.AreEqual(0, m_clipboardHistory.Count);

            Assert.That(m_clipboardHistory.Load("{ 'asdf' : 'asdf' "), Is.False);
            Assert.AreEqual(0, m_clipboardHistory.Count);
        }
    }
}
