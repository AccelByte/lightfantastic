using UnityEngine;
using UnityEngine.UI;

public class UILeaderboardComponent : MonoBehaviour
{
    #region Register UI Fields
    [Header("Miscellaneous")]
    public ScrollRect leaderboardScrollView;
    public Transform leaderboardScrollContent;
    public Transform rankPrefab;

    public Text myNumberText;
    public Image myProfileIconImage;
    public Text myUsernameText;
    public Text myWinStatsText;

    [Header("Buttons")]
    public Button leaderboardButton;
    #endregion
}

