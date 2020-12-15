#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpFuture
    {
        public GgpFuture(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
