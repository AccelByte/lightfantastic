#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpPollType
    {
        public GgpPollType(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
