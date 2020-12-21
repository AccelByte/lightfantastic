#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpCpuAvailability
    {
        public GgpCpuAvailability(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
