#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpStreamSubscription
    {
        public GgpStreamSubscription(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
