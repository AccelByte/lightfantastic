#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpMouse
    {
        public GgpMouse(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
