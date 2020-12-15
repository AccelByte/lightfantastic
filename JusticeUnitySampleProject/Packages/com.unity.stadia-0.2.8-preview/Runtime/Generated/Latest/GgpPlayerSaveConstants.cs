#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public enum GgpPlayerSaveConstants
    {
        kGgpGameSettingsSlot = 1,
        kGgpFirstSaveGameSlot = 1000,
        kGgpLastSaveGameSlot = 1099,
        kGgpMaxSaveGames = 1099 - 1000 - 1,
        kGgpMaxQuotaSaveGameSize = 250 * 1024 * 1024,
        kGgpMaxQuotaSettingsSize = 1 * 1024 * 1024,
    }
}
#endif
