#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpClipboardDataTypeMask
    {
        public GgpClipboardDataTypeMask(uint value)
        {
            Value = value;
        }

        public uint Value;
    }
}
#endif
