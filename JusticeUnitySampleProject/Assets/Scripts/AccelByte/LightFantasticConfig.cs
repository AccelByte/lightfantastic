using System.Collections.Generic;

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
        testunity
    }

    public readonly static Dictionary<GAME_MODES, string> GAME_MODES_VERBOSE = new Dictionary<GAME_MODES,string>()
    {
        {GAME_MODES.testunity, "1 VS 1"}
    };

    public const int MATCHMAKING_FINDMATCH_TIMEOUT = 30;
    public const int WAITING_DEDICATED_SERVER_TIMEOUT = 120;
}
