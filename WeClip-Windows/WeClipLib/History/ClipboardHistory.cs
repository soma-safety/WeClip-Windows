using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Safety.WeClip.History
{
    public class ClipboardHistory : IDisposable
    {
        private readonly LinkedList<HistoryData> m_history = new LinkedList<HistoryData>();
        public int Count { get { return m_history.Count; } }
        public int Limit { get; private set; }

        public HistoryData CurrentClipboardData {  get { return m_history.First.Value; } }

        public ClipboardHistory()
        {
            Limit = 10;
        }

        public void Dispose()
        {
        }

        public bool AddNewClipData(ClipDataType clipDataType, string text)
        {
            if (m_history.Last != null && m_history.Last.Value.Equals(clipDataType, text) == true)
                return false;

            HistoryData historyData = new HistoryData();
            historyData.DataType = clipDataType;
            historyData.Data = text;

            m_history.AddLast(historyData);

            CheckHistoryCapacity();

            return true;
        }

        public void SetHistoryLimit(int limit)
        {
            Limit = limit;

            CheckHistoryCapacity();
        }

        private void CheckHistoryCapacity()
        {
            while (Count > Limit)
                m_history.RemoveFirst();
        }

        public bool Load(string jsonString)
        {
            try
            {
                JObject rootJObject = JObject.Parse(jsonString);
                JObject historyJObject = rootJObject["history"].ToObject<JObject>();

                IList<JToken> records = historyJObject["records"].Children().ToList();
                m_history.Clear();
                foreach (JToken record in records)
                {
                    HistoryData newHistory = new HistoryData();

                    newHistory.SeqNum = record["seq_num"].Value<int>();

                    ClipDataType clipDataType;
                    if (Enum.TryParse(record["type"].Value<string>(), out clipDataType) == false)
                        throw new ArgumentException();
                    newHistory.DataType = clipDataType;
                    newHistory.Data = HistoryData.ReadData(record["value"], newHistory.DataType);
                    newHistory.DeviceName = record["device"].Value<string>();

                    m_history.AddLast(newHistory);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
           
            return true;
        }
    }
}
