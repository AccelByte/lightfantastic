#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpFrameTokenMetricsEvent
    {
        public GgpFrameToken frame_token;
        public GgpFrameTokenMetrics metrics;
    }
}
#endif
