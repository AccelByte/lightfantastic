#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpMouseButton
    {
        public GgpMouseButton(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
