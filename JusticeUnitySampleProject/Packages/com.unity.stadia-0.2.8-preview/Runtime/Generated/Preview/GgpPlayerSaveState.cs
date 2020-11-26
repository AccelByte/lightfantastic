#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpPlayerSaveState
    {
        public GgpPlayerSaveState(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
