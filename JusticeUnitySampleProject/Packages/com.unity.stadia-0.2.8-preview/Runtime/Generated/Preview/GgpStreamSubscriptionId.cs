#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpStreamSubscriptionId
    {
        public GgpStreamSubscriptionId(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
