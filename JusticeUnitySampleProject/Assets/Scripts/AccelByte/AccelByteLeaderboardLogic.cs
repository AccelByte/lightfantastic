// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using System.Collections.Generic;
using UITools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AccelByteLeaderboardLogic : MonoBehaviour
{
    private GameObject UIHandler;
    private UILeaderboardComponent UIHandlerLeaderboardComponent;
    private UIElementHandler UIElementHandler;
    private Leaderboard abLeaderboard;

    private IDictionary<string, RankData> playerRankList;

    private string lastPlayerRank;
    private string leaderboardCode = LightFantasticConfig.LEADERBOARD_CODE;
    private bool isActionPhaseOver = false;
    private bool isLeaderboardUpdate = false;
    private bool isCheckingDisplayName = false;

    public struct RankData
    {
        public string userId;
        public string rank;
        public string playerName;
        public float winStats;

        public RankData(string userId, string rank, string playerName, float winStats)
        {
            this.userId = userId;
            this.rank = rank;
            this.playerName = playerName;
            this.winStats = winStats;
        }
    }

    private void Start()
    {
        AccelByteManager.Instance.LobbyLogic.onMatchOver += LobbyLogic_onMatchOver;
    }

    private void Update()
    {
        if (isLeaderboardUpdate)
        {
            isLeaderboardUpdate = false;
            foreach (var data in playerRankList.Keys)
            {
                AccelBytePlugin.GetUser().GetUserByUserId(playerRankList[data].userId, OnGetUserDisplayName);
            }
        }
        if (isCheckingDisplayName)
        {
            isCheckingDisplayName = false;
            RefreshLeaderboardUIPrefabs();
        }
    }

    private void LobbyLogic_onMatchOver()
    {
        isActionPhaseOver = true;
    }

    /// <summary>
    /// Get leaderboard service reference & setup UI.
    /// </summary>
    public void Init()
    {
        if (abLeaderboard == null) abLeaderboard = AccelBytePlugin.GetLeaderboard();
        playerRankList = new Dictionary<string, RankData>();

        RefreshUIHandler();
    }

    /// <summary>
    /// Refresh Leaderboard service reference from leaderboard service.
    /// </summary>
    private void UpdateLeaderboardUI()
    {
        if (abLeaderboard == null) abLeaderboard = AccelBytePlugin.GetLeaderboard();
        playerRankList = new Dictionary<string, RankData>();
    }

    #region UI Listeners
    void OnEnable()
    {
        Debug.Log("ABLeaderboards OnEnable called!");

        // Register to onsceneloaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        Debug.Log("ABLeaderboards OnDisable called!");

        // Register to onsceneloaded
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (UIHandler != null)
        {
            RemoveListeners();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("ABLeaderboards OnSceneLoaded level loaded!");

        RefreshUIHandler();
    }

    public void RefreshUIHandler()
    {
        UIHandler = GameObject.FindGameObjectWithTag("UIHandler");
        if (UIHandler == null)
        {
            Debug.Log("ABStatistics RefreshUIHandler no reference to UI Handler!");
            return;
        }
        UIHandlerLeaderboardComponent = UIHandler.GetComponent<UILeaderboardComponent>();
        UIElementHandler = UIHandler.GetComponent<UIElementHandler>();
        abLeaderboard = AccelBytePlugin.GetLeaderboard();

        AddEventListeners();

        if (isActionPhaseOver)
        {
            UpdateLeaderboardUI();
            isActionPhaseOver = false;
        }

    }

    void AddEventListeners()
    {
        Debug.Log("ABLeaderboards AddEventListeners!");
        // Bind Buttons
        UIHandlerLeaderboardComponent.leaderboardButton.onClick.AddListener(GetLeaderboard);
    }

    void RemoveListeners()
    {
        Debug.Log("ABLeaderboards RemoveListeners!");
        UIHandlerLeaderboardComponent.leaderboardButton.onClick.RemoveListener(GetLeaderboard);
    }
    #endregion // UI Listeners

    #region AccelByte Leaderboard Functions

    /// <summary>
    /// Get user personal rank, top 10 ranks on the leaderboard.
    /// </summary>
    public void GetLeaderboard()
    {
        UIHandlerLeaderboardComponent.myUsernameText.text = AccelByteManager.Instance.AuthLogic.GetUserData().displayName;

        playerRankList.Clear();
        GetMyRanking();
        GetTopTenRanking();
    }

    private void GetMyRanking()
    {
        abLeaderboard.GetUserRanking(AccelByteManager.Instance.AuthLogic.GetUserData().userId, leaderboardCode ,OnGetMyRanking);
    }

    private void GetTopTenRanking()
    {
        abLeaderboard.QueryAllTimeLeaderboardRankingData(leaderboardCode, 0, 10, OnGetTopTenRanking);
    }

    private void ClearLeaderboardUIPrefabs()
    {
        if (UIHandlerLeaderboardComponent.leaderboardScrollContent.childCount > 0)
        {
            for (int i = 0; i < UIHandlerLeaderboardComponent.leaderboardScrollContent.childCount; i++)
            {
                Destroy(UIHandlerLeaderboardComponent.leaderboardScrollContent.GetChild(i).gameObject);
            }
        }
    }

    private void RefreshLeaderboardUIPrefabs()
    {
        ClearLeaderboardUIPrefabs();
        foreach (KeyValuePair<string, RankData> player in playerRankList)
        {
            RankPrefab rankPrefab = Instantiate(UIHandlerLeaderboardComponent.rankPrefab, Vector3.zero, Quaternion.identity).GetComponent<RankPrefab>();
            rankPrefab.transform.SetParent(UIHandlerLeaderboardComponent.leaderboardScrollContent, false);

            rankPrefab.GetComponent<RankPrefab>().SetupLeaderboardUI(player.Value.rank, player.Value.playerName, player.Value.winStats.ToString());

            UIHandlerLeaderboardComponent.leaderboardScrollView.Rebuild(CanvasUpdate.Layout);

            if (player.Value.playerName == "Loading . . .")
            {
                isCheckingDisplayName = true;
            }
        }
    }
    #endregion

    #region AccelByte Leaderboard Callbacks
    /// <summary>
    /// Callback from get user personal rank and update the UI
    /// </summary>
    /// <param name="result"> Result callback by get personal rank method, it returns alltime daily weekly monthly rank</param>
    private void OnGetMyRanking(Result<UserRankingData> result)
    {
        if (result.IsError)
        {
            Debug.Log("Get user ranking failed with LeaderboardCode: " + leaderboardCode + " Error : " + result.Error.Message);
        }
        else
        {
            string rankPlayer = result.Value.allTime.rank.ToString();
            if (result.Value.allTime.rank < 9)
            {
                rankPlayer = "0" + rankPlayer;
            }

            UIHandlerLeaderboardComponent.myNumberText.text = rankPlayer;
            UIHandlerLeaderboardComponent.myWinStatsText.text = result.Value.allTime.point.ToString();
        }
    }

    /// <summary>
    /// Callback from get top 10 ranks and update the UI
    /// </summary>
    /// <param name="result"></param>
    private void OnGetTopTenRanking(Result<LeaderboardRankingResult> result)
    {
        if (result.IsError)
        {
            Debug.Log("Query leaderboard failed with LeaderboardCode: " + leaderboardCode + " Error : " + result.Error.Message);
        }
        else
        {

            for (int i = 0;  i < result.Value.data.Length; i++)
            {
                if (!playerRankList.ContainsKey(result.Value.data[i].userId))
                {
                    var playerResult = result.Value.data[i];
                    string playerRankResult = (i + 1).ToString();
                    if (i < 9)
                    {
                        playerRankResult = "0" + playerRankResult;
                    }

                    playerRankList.Add(playerResult.userId, new RankData(playerResult.userId, playerRankResult, "Loading . . .", playerResult.point));
                    lastPlayerRank = playerResult.userId;
                }
            }

            isLeaderboardUpdate = true;
            RefreshLeaderboardUIPrefabs();
        }
    }

    /// <summary>
    /// Callback from Get user display name after getting the user id then apply it to the UI
    /// </summary>
    /// <param name="result"> Result callback userdata then get only the display name</param>
    private void OnGetUserDisplayName(Result<UserData> result)
    {
        if (result.IsError)
        {
            Debug.Log("Get user display name failed:" + result.Error.Message);
        }
        else
        {
            string playerUserId = result.Value.userId;
            playerRankList[playerUserId] = new RankData(playerUserId, playerRankList[playerUserId].rank, result.Value.displayName, playerRankList[playerUserId].winStats);

            if (result.Value.userId == lastPlayerRank)
            {
                RefreshLeaderboardUIPrefabs();
            }
        }
    }
    #endregion
}
