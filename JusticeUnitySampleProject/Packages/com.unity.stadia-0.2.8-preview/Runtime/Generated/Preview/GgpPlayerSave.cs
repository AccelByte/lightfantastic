#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpPlayerSave
    {
        public GgpPlayerSave(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
