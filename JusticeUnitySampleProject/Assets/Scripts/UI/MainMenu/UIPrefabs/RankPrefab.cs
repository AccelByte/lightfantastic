// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

#pragma warning disable 0649

using AccelByte.Core;
using UnityEngine;
using UnityEngine.UI;

public class RankPrefab : MonoBehaviour
{
    [SerializeField]
    private Text numberText;
    [SerializeField]
    private Image playerIconImage;
    [SerializeField]
    private Text usernameText;
    [SerializeField]
    private Text WinStatsText;

    public void SetupLeaderboardUI(string numberRank, string username, string WinStats)
    {
        numberText.text = numberRank;
        usernameText.text = username;
        WinStatsText.text = WinStats;
    }
}
