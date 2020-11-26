#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpMicAudioMuteState
    {
        public GgpMicAudioMuteState(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
