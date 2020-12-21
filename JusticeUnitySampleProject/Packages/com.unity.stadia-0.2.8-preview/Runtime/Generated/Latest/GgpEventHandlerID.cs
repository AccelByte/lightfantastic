#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpEventHandlerID
    {
        public GgpEventHandlerID(uint value)
        {
            Value = value;
        }

        public uint Value;
    }
}
#endif
