#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpGamepadButtonMask
    {
        public GgpGamepadButtonMask(uint value)
        {
            Value = value;
        }

        public uint Value;
    }
}
#endif
