#if ENABLE_GGP_PREVIEW_APIS
using System;
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    public static partial class StadiaNativeApis
    {
        private const string libraryPath = "libunitystadia_preview.so";

        [DllImport(libraryPath, EntryPoint = "GgpInitiateAccountLinking", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpInitiateAccountLinking(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpBeginAccountLinking", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpBeginAccountLinking(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpEndAccountLinking", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpEndAccountLinking(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpListAchievements", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpListAchievements(GgpPlayerId player_id);

        [DllImport(libraryPath, EntryPoint = "GgpGetAchievement", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpGetAchievement(GgpPlayerId player_id, GgpAchievementId achievement_id);

        [DllImport(libraryPath, EntryPoint = "GgpAchievementGetPlayerId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerId GgpAchievementGetPlayerId(GgpAchievement achievement, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAchievementGetId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpAchievementId GgpAchievementGetId(GgpAchievement achievement, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAchievementGetType", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpAchievementType GgpAchievementGetType(GgpAchievement achievement, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAchievementGetName", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpAchievementGetName(GgpAchievement achievement, [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder out_name, long name_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAchievementGetDescription", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpAchievementGetDescription(GgpAchievement achievement, [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder out_description, long description_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAchievementGetProgress", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GgpAchievementGetProgress(GgpAchievement achievement, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAchievementGetCompletionTimestamp", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpTimestampMicroseconds GgpAchievementGetCompletionTimestamp(GgpAchievement achievement, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAchievementGetInfo", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpAchievementGetInfo(GgpAchievement achievement, out GgpAchievementInfo out_achievement_info, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAchievementUpdateProgress", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpAchievementUpdateProgress(GgpAchievement achievement, int progress_percentage, out GgpTimestampMicroseconds out_completion_timestamp, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddPlayerConnectedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddPlayerConnectedHandler(GgpEventQueue queue, GgpPlayerConnectedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemovePlayerConnectedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemovePlayerConnectedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplacePlayerConnectedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplacePlayerConnectedHandler(GgpEventHandle handle, GgpPlayerConnectedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddStreamStateChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddStreamStateChangedHandler(GgpEventQueue queue, GgpStreamStateChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveStreamStateChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveStreamStateChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceStreamStateChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceStreamStateChangedHandler(GgpEventHandle handle, GgpStreamStateChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpStopStream", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GgpStopStream();

        [DllImport(libraryPath, EntryPoint = "GgpGetStreamState", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpStreamState GgpGetStreamState();

        [DllImport(libraryPath, EntryPoint = "GgpAddFocusChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddFocusChangedHandler(GgpEventQueue queue, GgpFocusChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveFocusChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveFocusChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceFocusChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceFocusChangedHandler(GgpEventHandle handle, GgpFocusChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpHasFocus", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpHasFocus();

        [DllImport(libraryPath, EntryPoint = "GgpGetClientState", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpClientState GgpGetClientState();

        [DllImport(libraryPath, EntryPoint = "GgpHelperGetDirectory", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpHelperGetDirectory([MarshalAs(UnmanagedType.LPStr)] string src_path, [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder out_path, long path_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetGameDataDirectory", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGetGameDataDirectory([MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder out_path, long path_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetAssetDirectory", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGetAssetDirectory([MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder out_path, long path_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAudioEndpointGetType", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpAudioEndpointType GgpAudioEndpointGetType(GgpAudioEndpoint endpoint, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAudioEndpointGetChannelCount", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpAudioEndpointGetChannelCount(GgpAudioEndpoint endpoint, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAudioEndpointGetPulseAudioDeviceName", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpAudioEndpointGetPulseAudioDeviceName(GgpAudioEndpoint endpoint, [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder out_device_name, long device_name_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAudioEndpointIsEnabled", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpAudioEndpointIsEnabled(GgpAudioEndpoint endpoint);

        [DllImport(libraryPath, EntryPoint = "GgpAddAudioEndpointChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddAudioEndpointChangedHandler(GgpEventQueue queue, GgpAudioEndpointChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveAudioEndpointChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveAudioEndpointChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceAudioEndpointChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceAudioEndpointChangedHandler(GgpEventHandle handle, GgpAudioEndpointChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetGameAudioEndpoint", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpAudioEndpoint GgpGetGameAudioEndpoint(out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetVoiceChatEndpoint", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpAudioEndpoint GgpGetVoiceChatEndpoint(out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpEnableVoiceChatEndpoint", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GgpEnableVoiceChatEndpoint([MarshalAs(UnmanagedType.U1)] bool enable);

        [DllImport(libraryPath, EntryPoint = "GgpGetJwtForPlayerInternal", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpGetJwtForPlayerInternal(GgpPlayerId player_id, long min_time_to_live_sec, GgpJwtFields extra_fields_mask);

        [DllImport(libraryPath, EntryPoint = "GgpGetJwtForPlayer", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpGetJwtForPlayer(GgpPlayerId player_id, long min_time_to_live_sec, GgpJwtFields extra_fields_mask);

        [DllImport(libraryPath, EntryPoint = "GgpGetClientTime", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGetClientTime(GgpTimestampMicroseconds time, out GgpTm out_client_time, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetClientDisplayTime", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpTm GgpGetClientDisplayTime(long time);

        [DllImport(libraryPath, EntryPoint = "GgpReadClipboard", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpReadClipboard(GgpPlayerId player_id, GgpClipboardDataTypeMask type_mask);

        [DllImport(libraryPath, EntryPoint = "GgpWriteClipboard", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpWriteClipboard(GgpPlayerId player_id, ref GgpClipboardItem items, long items_count);

        [DllImport(libraryPath, EntryPoint = "GgpGetClipboardReadStatus", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpClipboardStatus GgpGetClipboardReadStatus(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetClipboardWriteStatus", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpClipboardStatus GgpGetClipboardWriteStatus(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpHasClipboard", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpHasClipboard(GgpPlayerId player_id);

        [DllImport(libraryPath, EntryPoint = "GgpGetMonotonicTime", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpMicroseconds GgpGetMonotonicTime();

        [DllImport(libraryPath, EntryPoint = "GgpConsumeEntitlement", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpConsumeEntitlement(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPStr)] string entitlement_transaction_id);

        [DllImport(libraryPath, EntryPoint = "GgpListEntitlements", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpListEntitlements(GgpPlayerId player_id);

        [DllImport(libraryPath, EntryPoint = "GgpListProductItems", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpListProductItems(GgpPlayerId player_id);

        [DllImport(libraryPath, EntryPoint = "GgpAddEntitlementChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddEntitlementChangedHandler(GgpEventQueue queue, GgpEntitlementChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveEntitlementChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveEntitlementChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceEntitlementChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceEntitlementChangedHandler(GgpEventHandle handle, GgpEntitlementChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpShowPurchaseOverlay", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpShowPurchaseOverlay(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPStr)] string product_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpShowStoreOverlay", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpShowStoreOverlay(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpPollCreateChatPollAsync", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpPollCreateChatPollAsync(GgpPlayerId player_id, ref GgpPollOption options, long option_count);

        [DllImport(libraryPath, EntryPoint = "GgpPollCreateMultipleChoicePollAsync", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpPollCreateMultipleChoicePollAsync(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPStr)] string prompt_question, ref GgpPollOption options, long option_count);

        [DllImport(libraryPath, EntryPoint = "GgpPollDestroyAsync", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpPollDestroyAsync(GgpPoll poll);

        [DllImport(libraryPath, EntryPoint = "GgpEnableCommunityPoll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GgpEnableCommunityPoll();

        [DllImport(libraryPath, EntryPoint = "GgpIsCommunityPollActive", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpIsCommunityPollActive(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddCommunityPollChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddCommunityPollChangedHandler(GgpEventQueue queue, GgpCommunityPollChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveCommunityPollChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveCommunityPollChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceCommunityPollChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceCommunityPollChangedHandler(GgpEventHandle handle, GgpCommunityPollChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpPollCreateChatPoll", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpPollCreateChatPoll(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] GgpPollOption[] options, long option_count);

        [DllImport(libraryPath, EntryPoint = "GgpPollCreateMultipleChoicePoll", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpPollCreateMultipleChoicePoll(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPStr)] string prompt_question, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpPollOption[] options, long option_count);

        [DllImport(libraryPath, EntryPoint = "GgpPollDestroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpPollDestroy(GgpPoll poll);

        [DllImport(libraryPath, EntryPoint = "GgpPollIsActive", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpPollIsActive(GgpPoll poll);

        [DllImport(libraryPath, EntryPoint = "GgpPollGetType", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPollType GgpPollGetType(GgpPoll poll);

        [DllImport(libraryPath, EntryPoint = "GgpPollQuery", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpPollQuery(GgpPoll poll, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] long[] out_results, long result_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpBeginPoll", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPoll GgpBeginPoll(GgpPlayerId player_id, GgpPollType poll_type, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpPollOption[] options, long option_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpEndPoll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpEndPoll(GgpPoll poll, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] long[] out_results, long result_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpIsPollActive", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpIsPollActive(GgpPoll poll);

        [DllImport(libraryPath, EntryPoint = "GgpQueryPoll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpQueryPoll(GgpPoll poll, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] long[] out_results, long result_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetConsentStatus", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpConsentStatus GgpGetConsentStatus(GgpPlayerId player_id, GgpConsentType consent_type, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpEventQueueCreate", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventQueue GgpEventQueueCreate();

        [DllImport(libraryPath, EntryPoint = "GgpEventQueueDestroy", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpEventQueueDestroy(GgpEventQueue queue, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpEventQueueProcess", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpEventQueueProcess(GgpEventQueue queue, GgpMicroseconds wait_time_us);

        [DllImport(libraryPath, EntryPoint = "GgpEventQueueProcessEvent", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpEventQueueProcessEvent(GgpEventQueue queue, uint wait_time_ms);

        [DllImport(libraryPath, EntryPoint = "GgpIsFeatureRestricted", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpIsFeatureRestricted(GgpPlayerId player_id, GgpGamerFeature feature, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpShowFeatureRestrictionOverlay", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpShowFeatureRestrictionOverlay(GgpPlayerId player_id, GgpGamerFeature feature, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpIssueFrameToken", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFrameToken GgpIssueFrameToken();

        [DllImport(libraryPath, EntryPoint = "GgpAddFrameTokenMetricsHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandlerID GgpAddFrameTokenMetricsHandler(GgpEventQueue queue, GgpFrameTokenMetricsHandler handler, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveFrameTokenMetricsHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveFrameTokenMetricsHandler(GgpEventHandlerID id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceFrameTokenMetricsHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceFrameTokenMetricsHandler(GgpEventHandlerID id, GgpFrameTokenMetricsHandler handler, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetFrameTokenMetrics", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGetFrameTokenMetrics(GgpFrameToken frame_token, out GgpFrameTokenMetrics out_metrics, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpFutureGetResult", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpFutureGetResult(GgpFuture future, IntPtr result, long result_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpFutureGetResultCount", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpFutureGetResultCount(GgpFuture future, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpFutureGetResultSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpFutureGetResultSize(GgpFuture future, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpFutureGetMultipleResult", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpFutureGetMultipleResult(GgpFuture future, IntPtr result, long result_size, long max_count, out long result_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpFutureIsReady", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpFutureIsReady(GgpFuture future);

        [DllImport(libraryPath, EntryPoint = "GgpFutureDetach", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GgpFutureDetach(GgpFuture future);

        [DllImport(libraryPath, EntryPoint = "GgpFutureGet", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpFutureGet(GgpFuture future, IntPtr result, long result_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpFutureResultCount", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpFutureResultCount(GgpFuture future, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpFutureResultSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpFutureResultSize(GgpFuture future, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpFutureGetMultiple", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpFutureGetMultiple(GgpFuture future, IntPtr result, long result_size, long max_count, out long result_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetGameletConfig", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGetGameletConfig(out GgpGameletConfig out_config, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddGamepadHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddGamepadHotplugHandler(GgpEventQueue queue, GgpGamepadHotplugHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveGamepadHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveGamepadHotplugHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceGamepadHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceGamepadHotplugHandler(GgpEventHandle handle, GgpGamepadHotplugHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddGamepadInputHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddGamepadInputHandler(GgpEventQueue queue, GgpGamepadInputHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveGamepadInputHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveGamepadInputHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceGamepadInputHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceGamepadInputHandler(GgpEventHandle handle, GgpGamepadInputHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamepadIsConnected", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGamepadIsConnected(GgpGamepad gamepad);

        [DllImport(libraryPath, EntryPoint = "GgpGamepadSetVibration", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGamepadSetVibration(GgpGamepad gamepad, ref GgpGamepadVibration vibration, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamepadGetPlayerId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerId GgpGamepadGetPlayerId(GgpGamepad gamepad, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamepadGetConnected", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGamepadGetConnected([MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] GgpGamepad[] out_gamepads, long gamepads_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamepadGetType", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpGamepadType GgpGamepadGetType(GgpGamepad gamepad, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamepadHasDs4TrackpadButton", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGamepadHasDs4TrackpadButton(GgpGamepad gamepad);

        [DllImport(libraryPath, EntryPoint = "GgpAddGamepadPlayerChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddGamepadPlayerChangedHandler(GgpEventQueue queue, GgpGamepadPlayerChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveGamepadPlayerChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveGamepadPlayerChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceGamepadPlayerChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceGamepadPlayerChangedHandler(GgpEventHandle handle, GgpGamepadPlayerChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamepadGetCapabilities", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGamepadGetCapabilities(GgpGamepad gamepad, out GgpGamepadCapabilities capabilities, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpListGamerStats", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpListGamerStats(GgpPlayerId player_id);

        [DllImport(libraryPath, EntryPoint = "GgpListGamerStatsInNamespace", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpListGamerStatsInNamespace(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPStr)] string gamer_stat_namespace);

        [DllImport(libraryPath, EntryPoint = "GgpGetGamerStat", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpGetGamerStat(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPStr)] string gamer_stat_id);

        [DllImport(libraryPath, EntryPoint = "GgpGetGamerStatInNamespace", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpGetGamerStatInNamespace(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPStr)] string gamer_stat_namespace, [MarshalAs(UnmanagedType.LPStr)] string gamer_stat_id);

        [DllImport(libraryPath, EntryPoint = "GgpGamerStatGetPlayerId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerId GgpGamerStatGetPlayerId(GgpGamerStat gamer_stat, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamerStatGetNamespace", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGamerStatGetNamespace(GgpGamerStat gamer_stat, [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder out_namespace, long namespace_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamerStatGetId", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGamerStatGetId(GgpGamerStat gamer_stat, [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder out_id, long id_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamerStatIsVisible", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGamerStatIsVisible(GgpGamerStat gamer_stat, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamerStatGetName", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGamerStatGetName(GgpGamerStat gamer_stat, [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder out_name, long name_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamerStatGetDataType", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpGamerStatDataType GgpGamerStatGetDataType(GgpGamerStat gamer_stat, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamerStatGetInt", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGamerStatGetInt(GgpGamerStat gamer_stat, out long out_value, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamerStatGetDouble", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGamerStatGetDouble(GgpGamerStat gamer_stat, out double out_value, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamerStatGetString", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGamerStatGetString(GgpGamerStat gamer_stat, [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder out_value, long value_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamerStatGetInfo", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGamerStatGetInfo(GgpGamerStat gamer_stat, out GgpGamerStatInfo out_gamer_stat_info, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamerStatIsValid", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGamerStatIsValid(GgpGamerStat gamer_stat);

        [DllImport(libraryPath, EntryPoint = "GgpGamerStatSetInt", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGamerStatSetInt(GgpGamerStat gamer_stat, long value, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamerStatSetDouble", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGamerStatSetDouble(GgpGamerStat gamer_stat, double value, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGamerStatSetString", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGamerStatSetString(GgpGamerStat gamer_stat, [MarshalAs(UnmanagedType.LPStr)] string value, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpEnableGameBus", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpEnableGameBus(GgpPlayerId node_player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpCreateGameBusAddress", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpCreateGameBusAddress(out GgpGameBusMessageAddress out_address, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpCompareGameBusAddresses", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GgpCompareGameBusAddresses(ref GgpGameBusMessageAddress lhs_address, ref GgpGameBusMessageAddress rhs_address);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusEndpointCreate", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpGameBusEndpoint GgpGameBusEndpointCreate(ref GgpGameBusMessageAddress address, [MarshalAs(UnmanagedType.LPStr)] string purpose, GgpEventQueue queue, GgpGameBusMessageReceivedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusEndpointReplaceMessageReceivedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGameBusEndpointReplaceMessageReceivedHandler(GgpGameBusEndpoint endpoint, GgpGameBusMessageReceivedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusEndpointGetAddress", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGameBusEndpointGetAddress(GgpGameBusEndpoint endpoint, out GgpGameBusMessageAddress out_address, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusEndpointDestroy", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGameBusEndpointDestroy(GgpGameBusEndpoint endpoint, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusEndpointPublishMessage", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpGameBusEndpointPublishMessage(GgpGameBusEndpoint endpoint, ref GgpGameBusMessage message, GgpGameBusMessageScope scope, GgpMicroseconds message_lifetime_us);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusEndpointSubscribe", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGameBusEndpointSubscribe(GgpGameBusEndpoint endpoint, GgpGameBusMessageNamespace message_namespace, GgpGameBusMessageTypeId message_type_id, GgpGameBusMessageScope lower_bound_scope, GgpGameBusMessageScope upper_bound_scope, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusEndpointUnsubscribe", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGameBusEndpointUnsubscribe(GgpGameBusEndpoint endpoint, GgpGameBusMessageNamespace message_namespace, GgpGameBusMessageTypeId message_type_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusEndpointSendMessage", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpGameBusEndpointSendMessage(GgpGameBusEndpoint endpoint, ref GgpGameBusMessage message, GgpMicroseconds message_lifetime_us, ref GgpGameBusMessageAddress recipients, long recipients_count);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusEndpointAcquireMessage", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGameBusEndpointAcquireMessage(GgpGameBusEndpoint endpoint, ref GgpGameBusMessage message, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusEndpointReleaseMessage", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGameBusEndpointReleaseMessage(GgpGameBusEndpoint endpoint, ref GgpGameBusMessage message, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddGameBusPeersChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddGameBusPeersChangedHandler(GgpEventQueue queue, GgpGameBusPeersChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveGameBusPeersChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveGameBusPeersChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceGameBusPeersChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceGameBusPeersChangedHandler(GgpEventHandle handle, GgpGameBusPeersChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpListVisibleGameBusPeers", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpListVisibleGameBusPeers(out GgpGameBusPeer out_peers, long out_peers_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusPeerGetAddress", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGameBusPeerGetAddress(GgpGameBusPeer peer, out GgpGameBusMessageAddress out_address, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusPeerGetListenerPurpose", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGameBusPeerGetListenerPurpose(GgpGameBusPeer peer, [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder out_purpose, long purpose_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusPeerGetNodePlayerId", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGameBusPeerGetNodePlayerId(GgpGameBusPeer peer, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusPeerGetInfo", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGameBusPeerGetInfo(GgpGameBusPeer peer, out GgpGameBusPeerInfo out_info, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusAddTrackedLobby", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGameBusAddTrackedLobby(GgpLobby lobby, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusRemoveTrackedLobby", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGameBusRemoveTrackedLobby(GgpLobby lobby, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGameBusListTrackedLobbies", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGameBusListTrackedLobbies(out GgpLobby out_lobbies, long out_lobbies_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRecordGameEvent", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRecordGameEvent(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPStr)] string event_type_id, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpGameEventAttribute[] attributes, long attribute_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpMakeStringId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpStringId GgpMakeStringId([MarshalAs(UnmanagedType.LPStr)] string str);

        [DllImport(libraryPath, EntryPoint = "GgpGetInputMetadata", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGetInputMetadata(GgpInputToken input_token, out GgpInputMetadata out_input_metadata, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpBeginInputIdlePeriod", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpBeginInputIdlePeriod(GgpMicroseconds idle_period_us, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpEndInputIdlePeriod", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpEndInputIdlePeriod(out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpExternalInputReceived", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GgpExternalInputReceived();

        [DllImport(libraryPath, EntryPoint = "GgpGetClosestLanguage", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpGetClosestLanguage(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] GgpLanguageCodeString[] supported_languages, long supported_languages_count);

        [DllImport(libraryPath, EntryPoint = "GgpSetLanguage", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpSetLanguage(GgpPlayerId player_id, ref GgpLanguageCodeString language, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetCountryOfPurchase", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGetCountryOfPurchase(GgpPlayerId player_id, out GgpCountryCodeString out_country, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetCurrentCountry", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGetCurrentCountry(GgpPlayerId player_id, out GgpCountryCodeString out_country, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpKeyboardGetKeyState", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpKeyboardGetKeyState(ref GgpKeyboardInputEvent @event, GgpKeyCode key);

        [DllImport(libraryPath, EntryPoint = "GgpAddKeyboardHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddKeyboardHotplugHandler(GgpEventQueue queue, GgpKeyboardHotplugHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveKeyboardHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveKeyboardHotplugHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceKeyboardHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceKeyboardHotplugHandler(GgpEventHandle handle, GgpKeyboardHotplugHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddKeyboardInputHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddKeyboardInputHandler(GgpEventQueue queue, GgpKeyboardInputHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveKeyboardInputHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveKeyboardInputHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceKeyboardInputHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceKeyboardInputHandler(GgpEventHandle handle, GgpKeyboardInputHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddKeyboardCharacterHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddKeyboardCharacterHandler(GgpEventQueue queue, GgpKeyboardCharacterHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveKeyboardCharacterHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveKeyboardCharacterHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceKeyboardCharacterHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceKeyboardCharacterHandler(GgpEventHandle handle, GgpKeyboardCharacterHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpKeyboardIsConnected", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpKeyboardIsConnected(GgpKeyboard keyboard);

        [DllImport(libraryPath, EntryPoint = "GgpKeyboardGetPlayerId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerId GgpKeyboardGetPlayerId(GgpKeyboard keyboard, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpKeyboardGetConnected", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpKeyboardGetConnected([MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] GgpKeyboard[] out_keyboards, long keyboards_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpKeyboardGetCapabilities", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpKeyboardGetCapabilities(GgpKeyboard keyboard, out GgpKeyboardCapabilities capabilities, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpKeyboardGetType", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpKeyboardType GgpKeyboardGetType(GgpKeyboard keyboard, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddKeyboardPlayerChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddKeyboardPlayerChangedHandler(GgpEventQueue queue, GgpKeyboardPlayerChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveKeyboardPlayerChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveKeyboardPlayerChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceKeyboardPlayerChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceKeyboardPlayerChangedHandler(GgpEventHandle handle, GgpKeyboardPlayerChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLeaderboardCreate", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpLeaderboard GgpLeaderboardCreate(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPStr)] string gamer_stat_id, GgpLeaderboardSortOrder sort_order, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] GgpPlayerId[] player_ids, long player_ids_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLeaderboardCreateWithSettings", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpLeaderboard GgpLeaderboardCreateWithSettings(GgpPlayerId player_id, ref GgpLeaderboardSettings settings, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLeaderboardDestroy", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpLeaderboardDestroy(GgpLeaderboard leaderboard, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLeaderboardGetEntryCount", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLeaderboardGetEntryCount(GgpLeaderboard leaderboard);

        [DllImport(libraryPath, EntryPoint = "GgpLeaderboardGetEntriesFromRank", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLeaderboardGetEntriesFromRank(GgpLeaderboard leaderboard, long start_rank, long max_entries);

        [DllImport(libraryPath, EntryPoint = "GgpLeaderboardGetEntriesFromSelf", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLeaderboardGetEntriesFromSelf(GgpLeaderboard leaderboard, long offset, long max_entries);

        [DllImport(libraryPath, EntryPoint = "GgpIsLivestreamActive", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpIsLivestreamActive(GgpPlayerId player_id);

        [DllImport(libraryPath, EntryPoint = "GgpAddLivestreamChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddLivestreamChangedHandler(GgpEventQueue queue, GgpLivestreamChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveLivestreamChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveLivestreamChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceLivestreamChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceLivestreamChangedHandler(GgpEventHandle handle, GgpLivestreamChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpSetLogLevel", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GgpSetLogLevel(GgpLogLevel log_level);

        [DllImport(libraryPath, EntryPoint = "GgpGetLogLevel", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpLogLevel GgpGetLogLevel();

        [DllImport(libraryPath, EntryPoint = "GgpAddLogHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddLogHandler(GgpEventQueue queue, GgpLogHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveLogHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveLogHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceLogHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceLogHandler(GgpEventHandle handle, GgpLogHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetLogPath", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGetLogPath([MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder out_path, long path_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpSetLogPath", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpSetLogPath([MarshalAs(UnmanagedType.LPStr)] string file_path, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpEnableMediaRestriction", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GgpEnableMediaRestriction([MarshalAs(UnmanagedType.U1)] bool enable);

        [DllImport(libraryPath, EntryPoint = "GgpIsMediaRestrictionEnabled", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpIsMediaRestrictionEnabled();

        [DllImport(libraryPath, EntryPoint = "GgpGetNetworkDelayMicrosecondsForInput", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpMicroseconds GgpGetNetworkDelayMicrosecondsForInput(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetNetworkDelayMicrosecondsForVideo", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpMicroseconds GgpGetNetworkDelayMicrosecondsForVideo(out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetStreamFps", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GgpGetStreamFps(out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetStreamResolutionWidth", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GgpGetStreamResolutionWidth(out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetStreamResolutionHeight", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GgpGetStreamResolutionHeight(out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpSetStreamProfile", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpSetStreamProfile(ref GgpStreamProfile stream_profile, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetStreamProfile", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGetStreamProfile(out GgpStreamProfile out_stream_profile, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetStreamCapabilities", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGetStreamCapabilities(out GgpStreamCapabilities stream_capabilities, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpMicrophoneGetAudioData", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpMicrophoneGetAudioData(GgpMicrophone microphone, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] short[] out_pcm_buffer, long pcm_buffer_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpMicrophoneGetAudioDataAndMetadata", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpMicrophoneGetAudioDataAndMetadata(GgpMicrophone microphone, out short out_pcm_buffer, long pcm_buffer_count, out GgpMicrophoneMetadata out_metadata, long metadata_count, out long out_metadata_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpMicrophoneGetAvailableMetadataCount", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpMicrophoneGetAvailableMetadataCount(GgpMicrophone microphone, long num_samples, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpMicrophoneGetAvailableSampleCount", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpMicrophoneGetAvailableSampleCount(GgpMicrophone microphone, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddMicrophoneHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddMicrophoneHotplugHandler(GgpEventQueue queue, GgpMicrophoneHotplugHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveMicrophoneHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveMicrophoneHotplugHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceMicrophoneHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceMicrophoneHotplugHandler(GgpEventHandle handle, GgpMicrophoneHotplugHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpMicrophoneIsConnected", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpMicrophoneIsConnected(GgpMicrophone microphone);

        [DllImport(libraryPath, EntryPoint = "GgpMicrophoneGetConnected", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpMicrophoneGetConnected([MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] GgpMicrophone[] out_microphones, long microphones_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpMicrophoneGetPlayerId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerId GgpMicrophoneGetPlayerId(GgpMicrophone microphone, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddMicrophoneStateChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddMicrophoneStateChangedHandler(GgpEventQueue queue, GgpMicrophoneStateChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveMicrophoneStateChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveMicrophoneStateChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceMicrophoneStateChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceMicrophoneStateChangedHandler(GgpEventHandle handle, GgpMicrophoneStateChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddMicAudioStateChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddMicAudioStateChangedHandler(GgpEventQueue queue, GgpMicAudioStateChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveMicAudioStateChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveMicAudioStateChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceMicAudioStateChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceMicAudioStateChangedHandler(GgpEventHandle handle, GgpMicAudioStateChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetMicAudioAccessState", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpMicAudioAccessState GgpGetMicAudioAccessState(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpSetMicAudioMute", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpSetMicAudioMute(GgpPlayerId player_id, [MarshalAs(UnmanagedType.U1)] bool mute, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetMicAudioMuteState", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpMicAudioMuteState GgpGetMicAudioMuteState(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpSetMicAudioListening", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpSetMicAudioListening(GgpPlayerId player_id, [MarshalAs(UnmanagedType.U1)] bool listening, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpIsMicAudioListening", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpIsMicAudioListening(GgpPlayerId player_id);

        [DllImport(libraryPath, EntryPoint = "GgpMicrophoneIsLive", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpMicrophoneIsLive(GgpMicrophone microphone);

        [DllImport(libraryPath, EntryPoint = "GgpAddMouseHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddMouseHotplugHandler(GgpEventQueue queue, GgpMouseHotplugHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveMouseHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveMouseHotplugHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceMouseHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceMouseHotplugHandler(GgpEventHandle handle, GgpMouseHotplugHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddMouseInputHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddMouseInputHandler(GgpEventQueue queue, GgpMouseInputHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveMouseInputHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveMouseInputHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceMouseInputHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceMouseInputHandler(GgpEventHandle handle, GgpMouseInputHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpMouseIsConnected", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpMouseIsConnected(GgpMouse mouse);

        [DllImport(libraryPath, EntryPoint = "GgpMouseGetPlayerId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerId GgpMouseGetPlayerId(GgpMouse mouse, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpMouseGetConnected", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpMouseGetConnected([MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] GgpMouse[] out_mice, long mice_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddClientCursor", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpClientCursor GgpAddClientCursor(IntPtr image_data, int image_size, int hotspot_x, int hotspot_y, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpMouseSetCursor", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpMouseSetCursor(GgpMouse mouse, GgpClientCursor cursor, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpMouseSetCursorVisible", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpMouseSetCursorVisible(GgpMouse mouse, [MarshalAs(UnmanagedType.U1)] bool is_visible, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpMouseSetPosition", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpMouseSetPosition(GgpMouse mouse, int x, int y, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpMouseSetCoordinateMode", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpMouseSetCoordinateMode(GgpMouse mouse, GgpMouseCoordinateMode mode, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddMousePlayerChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddMousePlayerChangedHandler(GgpEventQueue queue, GgpMousePlayerChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveMousePlayerChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveMousePlayerChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceMousePlayerChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceMousePlayerChangedHandler(GgpEventHandle handle, GgpMousePlayerChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyCreate", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbyCreate(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPStr)] string content_type, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpLobbyProperty[] properties, long properties_count, GgpLobbyVisibility visibility);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyCreateForInvite", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbyCreateForInvite(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPStr)] string content_type, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpLobbyProperty[] properties, long properties_count, GgpMultiplayerInviteId invite_id);

        [DllImport(libraryPath, EntryPoint = "GgpGetLobbies", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpGetLobbies(GgpPlayerId player_id, ref GgpLobbyFilters lobby_filters);

        [DllImport(libraryPath, EntryPoint = "GgpGetLobbyByInviteId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpGetLobbyByInviteId(GgpPlayerId player_id, GgpMultiplayerInviteId invite_id);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyDelete", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpLobbyDelete(GgpLobby lobby, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyDeleteMember", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpLobbyDeleteMember(GgpLobby lobby, GgpPlayerId lobby_member_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyJoin", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbyJoin(GgpLobby lobby);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyLeave", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpLobbyLeave(GgpLobby lobby, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyGetPrivateInviteId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpMultiplayerInviteId GgpLobbyGetPrivateInviteId(GgpLobby lobby, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyGetHostPlayerId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerId GgpLobbyGetHostPlayerId(GgpLobby lobby, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyGetMemberIds", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpLobbyGetMemberIds(GgpLobby lobby, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] GgpPlayerId[] out_lobby_member_ids, long lobby_members_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyGetMemberAddress", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpLobbyGetMemberAddress(GgpLobby lobby, GgpPlayerId lobby_member_id, out GgpMultiplayerAddress out_address, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyGetMemberStreams", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpLobbyGetMemberStreams(GgpLobby lobby, GgpPlayerId lobby_member_id, out GgpStreamConnectionId out_connection_id, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] GgpStreamName[] out_names, long names_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyGetMemberProperty", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpLobbyGetMemberProperty(GgpLobby lobby, GgpPlayerId lobby_member_id, [MarshalAs(UnmanagedType.LPStr)] string property_name, out GgpLobbyProperty out_property, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyGetMemberLatency", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpLobbyGetMemberLatency(GgpLobby lobby, GgpPlayerId lobby_member_id, out GgpMicroseconds out_latency_us, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbySetMemberProperties", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbySetMemberProperties(GgpLobby lobby, GgpPlayerId lobby_member_id, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpLobbyProperty[] properties, long properties_count);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyPatchMemberProperties", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbyPatchMemberProperties(GgpLobby lobby, GgpPlayerId lobby_member_id, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpLobbyProperty[] properties_to_upsert, long properties_to_upsert_count, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=5)] IntPtr[] property_names_to_delete, long property_names_to_delete_count);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyGetProperty", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpLobbyGetProperty(GgpLobby lobby, [MarshalAs(UnmanagedType.LPStr)] string property_name, out GgpLobbyProperty out_property, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyGetMemberProperties", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpLobbyGetMemberProperties(GgpLobby lobby, GgpPlayerId lobby_member_id, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpLobbyProperty[] out_properties, long properties_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyGetProperties", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpLobbyGetProperties(GgpLobby lobby, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] GgpLobbyProperty[] out_properties, long properties_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbySetProperties", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbySetProperties(GgpLobby lobby, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] GgpLobbyProperty[] properties, long properties_count);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyPatchProperties", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbyPatchProperties(GgpLobby lobby, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] GgpLobbyProperty[] properties_to_upsert, long properties_to_upsert_count, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] IntPtr[] property_names_to_delete, long property_names_to_delete_count);

        [DllImport(libraryPath, EntryPoint = "GgpLobbySetVisibility", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbySetVisibility(GgpLobby lobby, GgpLobbyVisibility visibility);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyGetVisibility", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpLobbyGetVisibility(GgpLobby lobby, out GgpLobbyVisibility out_visibility, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbySetState", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbySetState(GgpLobby lobby, GgpLobbyState lobby_state);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyGetState", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpLobbyGetState(GgpLobby lobby, out GgpLobbyState out_lobby_state, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyAddInvite", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbyAddInvite(GgpLobby lobby, GgpMultiplayerInviteId invite_id);

        [DllImport(libraryPath, EntryPoint = "GgpAddLobbyMembersChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddLobbyMembersChangedHandler(GgpEventQueue queue, GgpLobbyMembersChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveLobbyMembersChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveLobbyMembersChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceLobbyMembersChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceLobbyMembersChangedHandler(GgpEventHandle handle, GgpLobbyMembersChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddLobbyDeletedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddLobbyDeletedHandler(GgpEventQueue queue, GgpLobbyDeletedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveLobbyDeletedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveLobbyDeletedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceLobbyDeletedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceLobbyDeletedHandler(GgpEventHandle handle, GgpLobbyDeletedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddLobbyChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddLobbyChangedHandler(GgpEventQueue queue, GgpLobbyChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveLobbyChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveLobbyChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceLobbyChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceLobbyChangedHandler(GgpEventHandle handle, GgpLobbyChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetMultiplayerListeningAddress", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGetMultiplayerListeningAddress(out GgpMultiplayerAddress out_address, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetMultiplayerExternalConnectionAddress", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGetMultiplayerExternalConnectionAddress(out GgpMultiplayerAddress out_address, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpIsQueueToPlayActive", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpIsQueueToPlayActive(GgpPlayerId player_id);

        [DllImport(libraryPath, EntryPoint = "GgpAddMultiplayerInviteHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddMultiplayerInviteHandler(GgpEventQueue queue, GgpMultiplayerInviteHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceMultiplayerInviteHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceMultiplayerInviteHandler(GgpEventHandle handle, GgpMultiplayerInviteHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveMultiplayerInviteHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveMultiplayerInviteHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpSendMultiplayerInvite", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpSendMultiplayerInvite(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] GgpPlayerId[] players, long players_count, ref GgpMultiplayerInviteContextString context);

        [DllImport(libraryPath, EntryPoint = "GgpShowMultiplayerInviteOverlay", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpShowMultiplayerInviteOverlay(GgpPlayerId player_id, long available_slots, ref GgpMultiplayerInviteContextString context, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpCreatePublicInvite", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpCreatePublicInvite(GgpPlayerId player_id, ref GgpOpenInviteState open_invite_state, [MarshalAs(UnmanagedType.LPStr)] string context);

        [DllImport(libraryPath, EntryPoint = "GgpReportOpenInvitePlayers", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReportOpenInvitePlayers(GgpPlayerId player_id, ref GgpOpenInviteState open_invite_state, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpDeleteMultiplayerInvite", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpDeleteMultiplayerInvite(GgpPlayerId player_id, GgpMultiplayerInviteId invite_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetActiveOpenInvite", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpMultiplayerInviteId GgpGetActiveOpenInvite(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpCreateLobby", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpCreateLobby(GgpPlayerId player_id, ref GgpMultiplayerValueString content_type, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpLobbyProperty[] properties, int properties_count, GgpLobbyVisibility visibility);

        [DllImport(libraryPath, EntryPoint = "GgpCreateLobbyForInvite", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpCreateLobbyForInvite(GgpPlayerId player_id, ref GgpMultiplayerValueString content_type, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpLobbyProperty[] properties, int properties_count, GgpMultiplayerInviteId invite_id);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyUpdateMemberProperties", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbyUpdateMemberProperties(GgpLobby lobby, GgpPlayerId lobby_member_id, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpLobbyProperty[] properties, int properties_count);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyManageMemberProperties", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbyManageMemberProperties(GgpLobby lobby, GgpPlayerId lobby_member_id, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpLobbyProperty[] properties_to_upsert, int properties_to_upsert_count, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=5)] GgpMultiplayerKeyString[] property_names_to_delete, int property_names_to_delete_count);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyUpdateProperties", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbyUpdateProperties(GgpLobby lobby, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] GgpLobbyProperty[] properties, int properties_count);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyManageProperties", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbyManageProperties(GgpLobby lobby, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] GgpLobbyProperty[] properties_to_upsert, int properties_to_upsert_count, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] GgpMultiplayerKeyString[] property_names_to_delete, int property_names_to_delete_count);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyUpdateVisibility", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbyUpdateVisibility(GgpLobby lobby, GgpLobbyVisibility visibility);

        [DllImport(libraryPath, EntryPoint = "GgpLobbyUpdateState", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpLobbyUpdateState(GgpLobby lobby, GgpLobbyState lobby_state);

        [DllImport(libraryPath, EntryPoint = "GgpMultiplayerGetListeningAddress", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpMultiplayerAddress GgpMultiplayerGetListeningAddress(out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpMultiplayerGetExternalConnectionAddress", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpMultiplayerAddress GgpMultiplayerGetExternalConnectionAddress(out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpListLobbies", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpListLobbies(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPStr)] string content_type, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpLobbyPropertyFilter[] property_filters, long property_filters_count);

        [DllImport(libraryPath, EntryPoint = "GgpReportQueuePlayers", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReportQueuePlayers(GgpPlayerId player_id, long total_player_slots, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpPlayerId[] players, long players_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetNetworkTopologyId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpGetNetworkTopologyId();

        [DllImport(libraryPath, EntryPoint = "GgpGetOrderedGcpCloudRegions", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpGetOrderedGcpCloudRegions(out IntPtr regions, long regions_count);

        [DllImport(libraryPath, EntryPoint = "GgpGetPrimaryPlayerId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerId GgpGetPrimaryPlayerId();

        [DllImport(libraryPath, EntryPoint = "GgpGetSecondaryPlayers", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGetSecondaryPlayers(out GgpPlayerId out_players, long players_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddSecondaryPlayerStateChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddSecondaryPlayerStateChangedHandler(GgpEventQueue queue, GgpSecondaryPlayerStateChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveSecondaryPlayerStateChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveSecondaryPlayerStateChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceSecondaryPlayerStateChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceSecondaryPlayerStateChangedHandler(GgpEventHandle handle, GgpSecondaryPlayerStateChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpDisconnectSecondaryPlayer", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpDisconnectSecondaryPlayer(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpListSaveGames", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpListSaveGames(GgpPlayerId player_id, GgpPlayerSaveSlot start_slot, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] GgpPlayerSave[] out_saves, long saves_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpCreateSaveGame", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpCreateSaveGame(GgpPlayerId player_id);

        [DllImport(libraryPath, EntryPoint = "GgpDeleteSaveGame", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpDeleteSaveGame(GgpPlayerId player_id, GgpPlayerSaveSlot save_slot);

        [DllImport(libraryPath, EntryPoint = "GgpGetSaveGame", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerSave GgpGetSaveGame(GgpPlayerId player_id, GgpPlayerSaveSlot save_slot, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetGameSettings", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerSave GgpGetGameSettings(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpPlayerSaveGetState", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerSaveState GgpPlayerSaveGetState(GgpPlayerSave save, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpPlayerSaveOpen", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpPlayerSaveOpen(GgpPlayerSave save, GgpPlayerSaveOpenMode mode);

        [DllImport(libraryPath, EntryPoint = "GgpPlayerSaveClose", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpPlayerSaveClose(GgpPlayerSave save, GgpPlayerSaveCloseMode mode);

        [DllImport(libraryPath, EntryPoint = "GgpPlayerSaveGetPlayerId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerId GgpPlayerSaveGetPlayerId(GgpPlayerSave save, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpPlayerSaveGetSlot", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerSaveSlot GgpPlayerSaveGetSlot(GgpPlayerSave save, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpPlayerSaveGetVersion", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GgpPlayerSaveGetVersion(GgpPlayerSave save, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpPlayerSaveGetTimestamp", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpTimestampMicroseconds GgpPlayerSaveGetTimestamp(GgpPlayerSave save, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpPlayerSaveGetValue", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpPlayerSaveGetValue(GgpPlayerSave save, [MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] out_value, long value_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpPlayerSaveSetValue", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpPlayerSaveSetValue(GgpPlayerSave save, [MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] value, long value_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpPlayerSaveDeleteValue", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpPlayerSaveDeleteValue(GgpPlayerSave save, [MarshalAs(UnmanagedType.LPStr)] string key, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpPlayerSaveGetQuotaUsage", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpPlayerSaveGetQuotaUsage(GgpPlayerSave save);

        [DllImport(libraryPath, EntryPoint = "GgpPlayerSaveGetSaveId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerSaveId GgpPlayerSaveGetSaveId(GgpPlayerSave save);

        [DllImport(libraryPath, EntryPoint = "GgpShowPlayerSignInOverlayForGamepad", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpShowPlayerSignInOverlayForGamepad(GgpGamepad gamepad, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpShowPlayerSignInOverlayForKeyboardAndMouse", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpShowPlayerSignInOverlayForKeyboardAndMouse(GgpKeyboard keyboard, GgpMouse mouse, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpShowPlayerSignInOverlay", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpShowPlayerSignInOverlay(out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpSetPlayerIdForGamepad", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpSetPlayerIdForGamepad(GgpGamepad gamepad, GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpSetPlayerIdForKeyboardAndMouse", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpSetPlayerIdForKeyboardAndMouse(GgpKeyboard keyboard, GgpMouse mouse, GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpBatchGetPresence", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpBatchGetPresence([MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] GgpPlayerId[] player_ids, long players_count);

        [DllImport(libraryPath, EntryPoint = "GgpGetProfile", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpGetProfile(GgpPlayerId player_id);

        [DllImport(libraryPath, EntryPoint = "GgpBatchGetProfile", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpBatchGetProfile([MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] GgpPlayerId[] player_ids, long players_count);

        [DllImport(libraryPath, EntryPoint = "GgpShowProfileOverlay", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpShowProfileOverlay(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpBatchGetProfileAvatar", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpBatchGetProfileAvatar(ref GgpProfileAvatarId avatar_ids, long avatar_ids_count);

        [DllImport(libraryPath, EntryPoint = "GgpListFriends", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpListFriends(GgpPlayerId player_id);

        [DllImport(libraryPath, EntryPoint = "GgpListBlockedPlayers", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpListBlockedPlayers(GgpPlayerId player_id);

        [DllImport(libraryPath, EntryPoint = "GgpBlockPlayer", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpBlockPlayer(GgpPlayerId player_id, GgpPlayerId player_to_block);

        [DllImport(libraryPath, EntryPoint = "GgpSendFriendInvite", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpSendFriendInvite(GgpPlayerId player_id, GgpPlayerId player_to_invite);

        [DllImport(libraryPath, EntryPoint = "GgpUnfriend", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpUnfriend(GgpPlayerId player_id, GgpPlayerId player_to_unfriend);

        [DllImport(libraryPath, EntryPoint = "GgpAddSocialGraphChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddSocialGraphChangedHandler(GgpEventQueue queue, GgpSocialGraphChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveSocialGraphChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveSocialGraphChangedHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceSocialGraphChangedHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceSocialGraphChangedHandler(GgpEventHandle handle, GgpSocialGraphChangedHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpSendInvite", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpSendInvite(GgpPlayerId player_id, GgpPlayerId player_to_invite);

        [DllImport(libraryPath, EntryPoint = "GgpGetStartupDataType", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpStartupDataType GgpGetStartupDataType();

        [DllImport(libraryPath, EntryPoint = "GgpGetStartupId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpStartupId GgpGetStartupId();

        [DllImport(libraryPath, EntryPoint = "GgpGetStartupGameState", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGetStartupGameState([MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] buffer, long buffer_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpSetActiveGameState", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpSetActiveGameState(GgpPlayerId player_id, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] byte[] data, long data_size, ref GgpGameStateMetadata metadata, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpClearActiveGameState", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpClearActiveGameState(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetStatusMessage", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGetStatusMessage(GgpStatusMessageToken message_token, [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder out_message, long message_size);

        [DllImport(libraryPath, EntryPoint = "GgpAddStatusMessage", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpStatusMessageToken GgpAddStatusMessage([MarshalAs(UnmanagedType.LPStr)] string message);

        [DllImport(libraryPath, EntryPoint = "GgpCreateStatus", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpStatus GgpCreateStatus(GgpStatusCode status_code, [MarshalAs(UnmanagedType.LPStr)] string message);

        [DllImport(libraryPath, EntryPoint = "GgpStatusGetMessage", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpStatusGetMessage(GgpStatusMessageId message_id, [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder out_message, long message_size);

        [DllImport(libraryPath, EntryPoint = "GgpStatusAddMessage", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpStatusMessageId GgpStatusAddMessage([MarshalAs(UnmanagedType.LPStr)] string message);

        [DllImport(libraryPath, EntryPoint = "GgpStatusCreate", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpStatus GgpStatusCreate(GgpStatusCode status_code, [MarshalAs(UnmanagedType.LPStr)] string message);

        [DllImport(libraryPath, EntryPoint = "GgpStreamSourceCreate", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpStreamSourceCreate(ref GgpStreamSourceCreateRequest create_request);

        [DllImport(libraryPath, EntryPoint = "GgpStreamSourceGetInfo", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpStreamSourceGetInfo(GgpStreamSource stream, out GgpStreamInfo out_stream_info, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpStreamSourceDestroy", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpStreamSourceDestroy(GgpStreamSource stream, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpStreamSubscriptionCreate", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpStreamSubscriptionCreate(ref GgpStreamSubscriptionCreateRequest create_request);

        [DllImport(libraryPath, EntryPoint = "GgpStreamSubscriptionGetInfo", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpStreamSubscriptionGetInfo(GgpStreamSubscription subscription, out GgpStreamInfo out_stream_info, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpStreamSubscriptionDestroy", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpStreamSubscriptionDestroy(GgpStreamSubscription subscription, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpStreamSubscriptionIsVideoFrameAvailable", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpStreamSubscriptionIsVideoFrameAvailable(GgpStreamSubscription subscription);

        [DllImport(libraryPath, EntryPoint = "GgpStreamSubscriptionAcquireNextVideoFrame", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpStreamSubscriptionAcquireNextVideoFrame(GgpStreamSubscription subscription, GgpVulkanQueue queue, GgpMicroseconds timeout_microseconds, GgpVulkanSemaphore image_ready_semaphore, GgpVulkanFence image_ready_fence, out int out_image_index, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpStreamSubscriptionGetFrameToken", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpStreamSubscriptionGetFrameToken(GgpStreamSubscription subscription, int image_index);

        [DllImport(libraryPath, EntryPoint = "GgpStreamSubscriptionRecordDecodeVideoFrameCommands", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpStreamSubscriptionRecordDecodeVideoFrameCommands(GgpStreamSubscription subscription, int image_index, GgpVulkanImageView target_image_view, GgpVulkanImageLayout target_image_layout, GgpVulkanCommandBuffer decode_command_buffer, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpStreamSubscriptionGetFrameMetadata", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpStreamSubscriptionGetFrameMetadata(GgpStreamSubscription subscription, GgpFrameToken frame_token, out byte buffer, long buffer_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpStreamSourceSetFrameMetadata", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpStreamSourceSetFrameMetadata(GgpStreamSource stream_source, GgpFrameToken frame_token, ref byte buffer, long buffer_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpStreamCreateSource", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpStreamCreateSource(ref GgpStreamSourceCreateRequest create_request);

        [DllImport(libraryPath, EntryPoint = "GgpStreamGetSource", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpStreamGetSource(GgpStreamDescriptor stream, out GgpStreamInfo out_stream_info, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpStreamDestroySource", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpStreamDestroySource(GgpStreamDescriptor stream, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpStreamGetSubscription", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpStreamGetSubscription(GgpStreamSubscriptionId subscription_id, out GgpStreamInfo out_stream_info, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpStreamDestroySubscription", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpStreamDestroySubscription(GgpStreamSubscriptionId subscription_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpStreamGetFrameMetadata", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpStreamGetFrameMetadata(GgpStreamSubscriptionId subscription_id, GgpFrameToken frame_token, out byte buffer, long buffer_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpStreamSetFrameMetadata", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpStreamSetFrameMetadata(GgpStreamDescriptor stream_descriptor, GgpFrameToken frame_token, ref byte buffer, long buffer_size, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetCpuCount", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGetCpuCount();

        [DllImport(libraryPath, EntryPoint = "GgpGetCpuAvailability", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpCpuAvailability GgpGetCpuAvailability(int cpu_index, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetCpusSharingCore", CallingConvention = CallingConvention.Cdecl)]
        public static extern long GgpGetCpusSharingCore(int cpu_index, out int out_cpus, long cpus_count, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddTextHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddTextHandler(GgpEventQueue queue, GgpTextHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveTextHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveTextHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceTextHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceTextHandler(GgpEventHandle handle, GgpTextHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpBeginText", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpBeginText(GgpPlayerId player_id, GgpTextEntryType type, ref GgpTextRegionOfInterest region_of_interest, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpEndText", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpEndText(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpShowTextPrompt", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpFuture GgpShowTextPrompt(GgpPlayerId player_id, GgpTextEntryType type, [MarshalAs(UnmanagedType.LPStr)] string title, [MarshalAs(UnmanagedType.LPStr)] string message, [MarshalAs(UnmanagedType.LPStr)] string initial_value, int max_value_length);

        [DllImport(libraryPath, EntryPoint = "GgpCancelTextPrompt", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpCancelTextPrompt(GgpPlayerId player_id, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpGetTextState", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpTextState GgpGetTextState(GgpPlayerId player_id);

        [DllImport(libraryPath, EntryPoint = "GgpAddTextEventHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddTextEventHandler(GgpEventQueue queue, GgpTextHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveTextEventHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveTextEventHandler(GgpEventHandle handle);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceTextEventHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceTextEventHandler(GgpEventHandle handle, GgpTextHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback);

        [DllImport(libraryPath, EntryPoint = "GgpIsTextActive", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpIsTextActive(GgpPlayerId player_id);

        [DllImport(libraryPath, EntryPoint = "GgpGetTextCapabilities", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpGetTextCapabilities(GgpPlayerId player_id, out GgpTextCapabilities out_capabilities, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddTouchHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddTouchHotplugHandler(GgpEventQueue queue, GgpTouchHotplugHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveTouchHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveTouchHotplugHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceTouchHotplugHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceTouchHotplugHandler(GgpEventHandle handle, GgpTouchHotplugHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpAddTouchInputHandler", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpEventHandle GgpAddTouchInputHandler(GgpEventQueue queue, GgpTouchInputHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpRemoveTouchInputHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpRemoveTouchInputHandler(GgpEventHandle handle, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpReplaceTouchInputHandler", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpReplaceTouchInputHandler(GgpEventHandle handle, GgpTouchInputHandler callback, IntPtr user_data, GgpUnregisterCallback unregister_callback, out GgpStatus out_status);

        [DllImport(libraryPath, EntryPoint = "GgpTouchIsConnected", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GgpTouchIsConnected(GgpTouch touch_device);

        [DllImport(libraryPath, EntryPoint = "GgpTouchGetPlayerId", CallingConvention = CallingConvention.Cdecl)]
        public static extern GgpPlayerId GgpTouchGetPlayerId(GgpTouch touch_device, out GgpStatus out_status);

}
}
#endif
