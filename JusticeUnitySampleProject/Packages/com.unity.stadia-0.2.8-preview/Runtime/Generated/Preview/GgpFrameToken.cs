#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpFrameToken
    {
        public GgpFrameToken(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
