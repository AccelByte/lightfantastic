#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpMicrophone
    {
        public GgpMicrophone(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
