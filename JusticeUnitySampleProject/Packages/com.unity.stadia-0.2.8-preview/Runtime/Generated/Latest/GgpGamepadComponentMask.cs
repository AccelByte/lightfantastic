#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpGamepadComponentMask
    {
        public GgpGamepadComponentMask(uint value)
        {
            Value = value;
        }

        public uint Value;
    }
}
#endif
