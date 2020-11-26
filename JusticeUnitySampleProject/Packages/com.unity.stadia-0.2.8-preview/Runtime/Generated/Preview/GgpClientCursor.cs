#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpClientCursor
    {
        public GgpClientCursor(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
