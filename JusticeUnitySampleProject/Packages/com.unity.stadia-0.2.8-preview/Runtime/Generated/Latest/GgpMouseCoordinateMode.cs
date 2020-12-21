#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpMouseCoordinateMode
    {
        public GgpMouseCoordinateMode(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
