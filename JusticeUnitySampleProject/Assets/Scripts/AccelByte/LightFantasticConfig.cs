﻿using System.Collections.Generic;

public static class LightFantasticConfig
{
    public const string DEFAULT_LANGUAGE = "en";
    public const string IMAGE_AS = "product-cover";

    public static class ItemTags
    {
        public const string hat = "hat";
        public const string effect = "effect";
    }

    public const float CURR_SPEED_MULTIPLIER_ANIMATION = 200.0f;
    public const bool DEVELOPER_CONSOLE_VISIBLE = false;
    public const string DS_LOCALMODE_CMD_ARG = "localds"; // Arguments passed to set this server behavior as local DS
    
    public enum GAME_MODES
    {
        unitytest = 0,
        upto4player = 1
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
        LINUX = 2
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
#endif
    }

    /// GAMEPLAY STUFF
    public static readonly uint COUNT_TO_START_RACE_SECOND = 4; // [ "GO", "1", "2", "3"]
    public static readonly uint RACE_LENGTH_SECOND = 30;
}
