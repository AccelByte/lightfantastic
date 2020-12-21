#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpLogEvent
    {
        public string category;
        public string method;
        public GgpMicroseconds latency_us;
        public GgpTimestampMicroseconds timestamp_us;
        public GgpStatus status;
    }
}
#endif
