#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpGameletMachine
    {
        public GgpGameletMachine(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
