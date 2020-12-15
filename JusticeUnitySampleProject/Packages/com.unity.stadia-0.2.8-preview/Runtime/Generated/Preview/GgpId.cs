#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpId
    {
        public GgpId(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
