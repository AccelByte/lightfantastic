#if ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpGameBusMessageReceivedEvent
    {
        public GgpGameBusEndpoint endpoint;
        public GgpGameBusMessageAddress sender;
        public IntPtr recipients;
        public long recipients_count;
        public IntPtr message;
        public GgpTimestampMicroseconds expire_time_us;
    }
}
#endif
