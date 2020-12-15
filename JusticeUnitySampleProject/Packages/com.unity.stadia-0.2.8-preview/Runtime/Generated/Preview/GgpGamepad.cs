#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpGamepad
    {
        public GgpGamepad(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
