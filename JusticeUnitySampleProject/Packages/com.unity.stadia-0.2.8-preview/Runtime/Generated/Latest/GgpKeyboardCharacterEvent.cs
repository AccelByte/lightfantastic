#if !ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    public partial struct GgpKeyboardCharacterEvent
    {
        public GgpKeyboard keyboard;
        public string character;
        [MarshalAs(UnmanagedType.U1)] public bool is_repeat;
    }
}
#endif
