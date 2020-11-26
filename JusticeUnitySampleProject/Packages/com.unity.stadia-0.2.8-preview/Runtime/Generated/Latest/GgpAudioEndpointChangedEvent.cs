#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpAudioEndpointChangedEvent
    {
        public GgpAudioEndpoint audio_endpoint;
        public GgpAudioEndpointType endpoint_type;
        public GgpAudioEndpointState endpoint_state;
    }
}
#endif
