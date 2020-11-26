#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpStreamStateChanged
    {
        public GgpStreamStateChanged(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
