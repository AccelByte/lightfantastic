#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpStreamSource
    {
        public GgpStreamSource(uint value)
        {
            Value = value;
        }

        public uint Value;
    }
}
#endif
