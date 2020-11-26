#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpPlayerId
    {
        public GgpPlayerId(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
