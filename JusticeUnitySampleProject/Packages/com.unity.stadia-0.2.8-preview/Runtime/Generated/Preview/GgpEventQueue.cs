#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpEventQueue
    {
        public GgpEventQueue(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
