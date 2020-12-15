#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpEventHandle
    {
        public GgpEventHandle(uint value)
        {
            Value = value;
        }

        public uint Value;
    }
}
#endif
