#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpGameBusPeer
    {
        public GgpGameBusPeer(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
