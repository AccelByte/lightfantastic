#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpStreamSourceCreateRequest
    {
        public string stream_name;
        public GgpVideoResolution video_resolution;
        public GgpSurfaceFormat surface_format;
        public int video_encode_cpu_threads;
        public int audio_channels;
    }
}
#endif
