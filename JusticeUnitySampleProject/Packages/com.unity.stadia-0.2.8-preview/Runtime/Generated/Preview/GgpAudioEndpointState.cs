#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpAudioEndpointState
    {
        public GgpAudioEndpointState(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
