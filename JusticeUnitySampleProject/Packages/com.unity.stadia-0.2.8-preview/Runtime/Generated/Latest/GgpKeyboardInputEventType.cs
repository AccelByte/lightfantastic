#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpKeyboardInputEventType
    {
        public GgpKeyboardInputEventType(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
