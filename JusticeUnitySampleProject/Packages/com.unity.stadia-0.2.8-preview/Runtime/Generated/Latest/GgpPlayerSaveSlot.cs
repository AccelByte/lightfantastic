#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpPlayerSaveSlot
    {
        public GgpPlayerSaveSlot(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
