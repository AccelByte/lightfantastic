#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpMicAudioAccessState
    {
        public GgpMicAudioAccessState(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
