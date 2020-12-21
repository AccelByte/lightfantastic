#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpGamepadType
    {
        public GgpGamepadType(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
