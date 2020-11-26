#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpClipboardStatus
    {
        public GgpClipboardStatus(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
