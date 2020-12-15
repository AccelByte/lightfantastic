#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpGameBusMessageTypeId
    {
        public GgpGameBusMessageTypeId(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
