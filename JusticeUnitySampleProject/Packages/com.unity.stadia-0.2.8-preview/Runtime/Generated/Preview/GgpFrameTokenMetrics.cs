#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpFrameTokenMetrics
    {
        public GgpMicroseconds issue_time_us;
        public GgpMicroseconds present_time_us;
        public GgpMicroseconds encode_start_time_us;
        public GgpMicroseconds encode_end_time_us;
        public GgpMicroseconds client_arrival_time_us;
        public GgpMicroseconds game_arrival_time_us;
    }
}
#endif
