﻿using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccelByteStatisticLogic : MonoBehaviour
{
    private User abUser;
    private Statistic statistic;

    #region Register UI Fields
    [SerializeField]
    private Text totalWinText;
    [SerializeField]
    private Text totalLoseText;
    [SerializeField]
    private Text totalMatchText;
    #endregion

    ICollection<string> playerStatistic;

    public void GetPlayerStatistic()
    {
        abUser = AccelBytePlugin.GetUser();

        playerStatistic = new List<string>
        {
            "TOTAL_WIN",
            "TOTAL_LOSE",
            "TOTAL_MATCH"
        };

        statistic = AccelBytePlugin.GetStatistic();
        statistic.GetUserStatItemsByStatCodes(playerStatistic, GetStatisticCallback);
    }

    #region AccelByte Statistic Callback
    public void GetStatisticCallback(Result<StatItemPagingSlicedResult> result)
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
                if (data.statCode == "TOTAL_WIN")
                {
                    totalWinText.text = data.value.ToString();
                }
                else if (data.statCode == "TOTAL_LOSE")
                {
                    totalLoseText.text = data.value.ToString();
                }
                else if (data.statCode == "TOTAL_MATCH")
                {
                    totalMatchText.text = data.value.ToString();
                }
            }
        }
    }

    #endregion
}