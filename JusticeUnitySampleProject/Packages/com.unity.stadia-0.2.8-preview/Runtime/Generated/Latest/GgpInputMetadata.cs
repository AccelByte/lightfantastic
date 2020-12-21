#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpInputMetadata
    {
        public GgpFrameToken frame_token_estimated;
        public GgpMicroseconds source_time_us;
    }
}
#endif
