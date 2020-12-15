#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpKeyCode
    {
        public GgpKeyCode(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
