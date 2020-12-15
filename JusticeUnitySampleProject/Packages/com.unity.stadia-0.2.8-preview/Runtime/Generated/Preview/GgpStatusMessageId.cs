#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpStatusMessageId
    {
        public GgpStatusMessageId(uint value)
        {
            Value = value;
        }

        public uint Value;
    }
}
#endif
