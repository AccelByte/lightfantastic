#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpInputToken
    {
        public GgpInputToken(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
