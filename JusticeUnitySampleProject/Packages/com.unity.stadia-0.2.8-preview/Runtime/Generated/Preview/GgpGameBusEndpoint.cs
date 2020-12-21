#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpGameBusEndpoint
    {
        public GgpGameBusEndpoint(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
