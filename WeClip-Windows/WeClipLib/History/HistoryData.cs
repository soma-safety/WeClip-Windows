using Newtonsoft.Json.Linq;

namespace Safety.WeClip.History
{
    public class HistoryData
    {
        public int SeqNum { get; set; }
        public ClipDataType DataType { get; set; }
        public object Data { get; set; }
        public string DeviceName { get; set; }

        public bool Equals(ClipDataType clipDataType, string text)
        {
            if (DataType != clipDataType)
                return false;

            if (Data != null && ((string)Data).CompareTo(text) != 0)
                return false;

            return true;
        }

        public static object ReadData(JToken jToken, ClipDataType dataType)
        {
            switch (dataType)
            {
                case ClipDataType.Text:
                    return jToken.Value<string>();
            }

            return null;
        }
    }
}