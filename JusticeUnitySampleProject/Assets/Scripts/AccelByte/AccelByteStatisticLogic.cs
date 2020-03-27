﻿﻿using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UITools;

public class AccelByteStatisticLogic : MonoBehaviour
{
    private Statistic abStatistic;

    private GameObject UIHandler;
    private UIStatisticsLogicComponent UIHandlerStatisticsComponent;
    private UIElementHandler UIElementHandler;

    private bool isActionPhaseOver = false;

    ICollection<string> playerStatistic;

    private void Start()
    {
        AccelByteManager.Instance.LobbyLogic.onMatchOver += LobbyLogic_onMatchOver;

        playerStatistic = new List<string>
        {
            LightFantasticConfig.StatisticCode.win,
            LightFantasticConfig.StatisticCode.lose,
            LightFantasticConfig.StatisticCode.total,
            LightFantasticConfig.StatisticCode.distance
        };
    }

    private void LobbyLogic_onMatchOver()
    {
        isActionPhaseOver = true;
    }

    public void UpdatePlayerStatisticUI()
    {
        if (abStatistic == null) abStatistic = AccelBytePlugin.GetStatistic();
        abStatistic.GetUserStatItems(playerStatistic, null, GetStatisticCallback);
    }

    #region UI Listeners
    void OnEnable()
    {
        Debug.Log("ABStatistics OnEnable called!");

        // Register to onsceneloaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        Debug.Log("ABStatistics OnDisable called!");

        // Register to onsceneloaded
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (UIHandler != null)
        {
            RemoveListeners();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("ABStatistics OnSceneLoaded level loaded!");

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
        UIHandlerStatisticsComponent = UIHandler.GetComponent<UIStatisticsLogicComponent>();
        UIElementHandler = UIHandler.GetComponent<UIElementHandler>();
        abStatistic = AccelBytePlugin.GetStatistic();

        AddEventListeners();

        if (isActionPhaseOver)
        {
            UpdatePlayerStatisticUI();
            isActionPhaseOver = false;
        }
    }

    void AddEventListeners()
    {
        Debug.Log("ABStatistics AddEventListeners!");
        // Bind Buttons
    }

    void RemoveListeners()
    {
        Debug.Log("ABStatistics RemoveListeners!");
    }
    #endregion // UI Listeners

    #region AccelByte Statistic Callback
    public void GetStatisticCallback(Result<PagedStatItems> result)
    {
        if (result.IsError)
        {
            Debug.Log("Get Statistic failed:" +  result.Error.Message);
            Debug.Log("Get Statistic Response Code: " +  result.Error.Code);
            //Show Error Message
        }
        else
        {
            Debug.Log("Get Statistic successful.");
            foreach (var data in result.Value.data)
            {
                if (data.statCode == LightFantasticConfig.StatisticCode.win)
                {
                    UIHandlerStatisticsComponent.totalWinText.text = data.value.ToString();
                }
                else if (data.statCode == LightFantasticConfig.StatisticCode.lose)
                {
                    UIHandlerStatisticsComponent.totalLoseText.text = data.value.ToString();
                }
                else if (data.statCode == LightFantasticConfig.StatisticCode.total)
                {
                    UIHandlerStatisticsComponent.totalMatchText.text = data.value.ToString();
                }
                else if (data.statCode == LightFantasticConfig.StatisticCode.distance)
                {
                    UIHandlerStatisticsComponent.totalDistanceText.text = data.value.ToString();
                }
            }
        }
    }

    #endregion
}