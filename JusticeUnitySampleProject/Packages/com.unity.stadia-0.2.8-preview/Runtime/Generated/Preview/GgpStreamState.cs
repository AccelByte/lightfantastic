#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpStreamState
    {
        public GgpStreamState(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
