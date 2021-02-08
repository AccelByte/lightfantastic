using System.Collections.Generic;
using AccelByte.Api;

public static class LightFantasticConfig
{
    public const string GAME_VERSION = "0.0.11";
    public const string DS_TARGET_VERSION = "v0.0.6";
    public const string SDK_VERSION = "2.28.1";
    
    public const string DEFAULT_LANGUAGE = "en";
    public const string IMAGE_AS = "product-cover";
    public const string PARTY_CHAT = "party";

    public static class ItemTags
    {
        public const string hat = "hat";
        public const string effect = "effect";
    }
    public const bool DEVELOPER_CONSOLE_VISIBLE = false;
    public const string DS_LOCALMODE_CMD_ARG = "localds"; // Arguments passed to set this server behavior as local DS
    
    public enum GAME_MODES
    {
        unitytest = 0,
        upto4player = 1
    }

    public static class StatisticCode
    {
        public const string win = "total-win";
        public const string lose = "total-lose";
        public const string total = "total-match";
        public const string distance = "total-distance";
    }

    public readonly static Dictionary<GAME_MODES, string> GAME_MODES_VERBOSE = new Dictionary<GAME_MODES,string>()
    {
        {GAME_MODES.upto4player, "4 Players FFA"},
        {GAME_MODES.unitytest, "1 VS 1"},
    };

    public const int MATCHMAKING_FINDMATCH_TIMEOUT = 15; // Should be less than 20 second to avoid ban
    public const int MATCHMAKING_CONFIRMING_READY_TIMEOUT = 10;
    public const int WAITING_DEDICATED_SERVER_TIMEOUT = 120;

    /// <summary>
    /// Tied to "Assets/Sprites/Character/CharacterHoverPlatforms.asset" library
    /// Category: "Platform"
    /// </summary>
    /// <returns></returns>
    public enum Platform
    {
        WINDOWS = 0,
        ANDROID = 1,
        LINUX = 2,
        STADIA = 3
    }

    public static readonly string PLATFORM_LIBRARY_ASSET_CATEGORY = "Platform";

    public static Platform GetPlatform()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return Platform.WINDOWS;
#elif UNITY_ANDROID
        return Platform.ANDROID;
#elif UNITY_STANDALONE_LINUX
        return Platform.LINUX;
#elif UNITY_STADIA
        return Platform.STADIA;
#endif
    }

    /// GAMEPLAY STUFF
    public static readonly uint COUNT_TO_START_RACE_SECOND = 4; // [ "GO", "1", "2", "3"]
    public static readonly uint RACE_LENGTH_SECOND = 30;
    public static readonly uint DEADLINE_TO_FORCE_START_MATCH_COUNTDOWN = 12 /*sec*/; // Start the race even though the player isn't ready

    public static readonly uint FINISH_LINE_DISTANCE = 500;
    
    public static readonly float PLAYER_SPEED_INCREASE = 0.015f;
    public static readonly float PLAYER_SPEED_DECAY = 0.1f;
    public static readonly float PLAYER_SPEED_DECAY_MULTIPLIER_ONFINISH = 1.02f;
    

    public const float CURR_SPEED_MULTIPLIER_ANIMATION = 200.0f;

    public static string GetPlayerPortalURL()
    {
        string baseUrl = AccelBytePlugin.Config.BaseUrl;
        if (!baseUrl.StartsWith("http"))
        {
            baseUrl = "https://" + baseUrl;
        }
        if (baseUrl.Contains("//api."))
        {
            baseUrl = baseUrl.Replace("//api.", "//");
        }
        return baseUrl;
    }
    
    public static readonly string LEADERBOARD_CODE = "alltimetotalwin";

    public static readonly string AUDIO_SETTING_KEY = "settingAudio";
    public static class AudioSettingType
    {
        public const string BGM = "settingAudioBGM";
        public const string SFX = "settingAudioSFX";
    }
}
