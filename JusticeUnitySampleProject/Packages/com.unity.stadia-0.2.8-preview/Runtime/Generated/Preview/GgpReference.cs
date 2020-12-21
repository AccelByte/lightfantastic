#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpReference
    {
        public GgpReference(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
