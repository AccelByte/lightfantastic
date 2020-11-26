#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpKeyboard
    {
        public GgpKeyboard(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
