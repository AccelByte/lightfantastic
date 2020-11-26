using System.Text;

namespace Unity.StadiaWrapper
{
    public static class GgpSystemExtensionMethods
    {
        public static GgpTm GetClientTime(this GgpTimestampMicroseconds ggpTimestampMicroseconds)
        {
            StadiaNativeApis.GgpGetClientTime(ggpTimestampMicroseconds, out GgpTm clientTime, out _);
            return clientTime;
        }
        
        public static bool IsOk(this GgpStatus ggpStatus)
        {
            return ggpStatus.status_code.Value == (int)GgpStatusCodeValues.kGgpStatusCode_Ok;
        }

        public static string GetStatusMessage(this GgpStatus ggpStatus)
        {
            StringBuilder stringBuilder = new StringBuilder();
            long messageSize = StadiaNativeApis.GgpGetStatusMessage(ggpStatus.message_token, stringBuilder, 0);
            stringBuilder = new StringBuilder((int)messageSize);
            StadiaNativeApis.GgpGetStatusMessage(ggpStatus.message_token, stringBuilder, messageSize);
            if (ggpStatus.IsOk() && stringBuilder.Length == 0)
            {
                stringBuilder.Append("Ok");
            }
            return stringBuilder.ToString();
        }
    }
}
