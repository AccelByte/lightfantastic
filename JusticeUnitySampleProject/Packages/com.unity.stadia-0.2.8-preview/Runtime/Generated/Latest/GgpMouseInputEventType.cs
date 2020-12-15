#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpMouseInputEventType
    {
        public GgpMouseInputEventType(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
