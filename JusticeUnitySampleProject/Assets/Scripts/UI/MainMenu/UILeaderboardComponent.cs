// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

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

