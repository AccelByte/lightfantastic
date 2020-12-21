#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpStartupId
    {
        public GgpStartupId(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
