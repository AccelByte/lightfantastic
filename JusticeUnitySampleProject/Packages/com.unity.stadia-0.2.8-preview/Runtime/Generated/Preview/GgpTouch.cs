#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpTouch
    {
        public GgpTouch(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
