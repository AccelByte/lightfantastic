#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public enum GgpPollConstants
    {
        kGgpMaxPollOptions = 25,
        kGgpPollOptionMaxTextSize = 100,
        kGgpMinMultipleChoicePollOptions = 2,
        kGgpMaxMultipleChoicePollOptions = 4,
        kGgpMultipleChoicePollPromptQuestionMaxTextSize = 400,
        kGgpMultipleChoicePollOptionMaxTextSize = 100,
    }
}
#endif
