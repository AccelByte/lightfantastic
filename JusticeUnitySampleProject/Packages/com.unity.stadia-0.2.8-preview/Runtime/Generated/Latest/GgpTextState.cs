#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpTextState
    {
        public GgpTextState(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
