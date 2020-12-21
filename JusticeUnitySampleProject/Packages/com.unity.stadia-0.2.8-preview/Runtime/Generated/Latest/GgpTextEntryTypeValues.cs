#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public enum GgpTextEntryTypeValues
    {
        kGgpTextEntryType_Invalid = 0,
        kGgpTextEntryType_SingleLine = 1,
        kGgpTextEntryType_MultipleLine = 2,
        kGgpTextEntryType_MultipleLineWithTab = 3,
        kGgpTextEntryType_Password = 4,
        kGgpTextEntryType_Number = 5,
        kGgpTextEntryType_Phone = 6,
        kGgpTextEntryType_Email = 7,
        kGgpTextEntryType_Uri = 8,
    }
}
#endif
