#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpStatusMessageToken
    {
        public GgpStatusMessageToken(uint value)
        {
            Value = value;
        }

        public uint Value;
    }
}
#endif
