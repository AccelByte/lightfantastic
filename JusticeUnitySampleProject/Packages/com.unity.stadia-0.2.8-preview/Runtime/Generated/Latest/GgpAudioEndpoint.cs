#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpAudioEndpoint
    {
        public GgpAudioEndpoint(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
