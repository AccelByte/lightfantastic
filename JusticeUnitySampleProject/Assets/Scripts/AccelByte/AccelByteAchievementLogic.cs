using AccelByte.Api;
using AccelByte.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelByteAchievementLogic : MonoBehaviour
{
    public PaginatedUserAchievement tempUserAchievement; // temporary UserAchievement, this variable is gonna compare with new UserAchievement to know if Incremental Achievement is Unlocked.
    public PaginatedUserAchievement newUserAchievement; // this variables is to compare new UserAchievement with temporary UserAchievement to know if Incremental Achievement is Unlocked.
    public PaginatedPublicAchievement publicAchievement; // public Achievement to get a detail info of Achievement
    private UserData localUserDataInfo; // This variables is to Check when Statistic Updated and Check If Incremental Achievement is Achieved with a spesific User.
    private float delayToWaitServerUpdated = 0.5f; // Delay for wait until Server Updated
    private int achievementStatusUnlocked = 2;

    /// <summary>
    /// Refresh when every MainMenuScene and GamePlayScene is loaded
    /// </summary>
    public void RefreshAchievement()
    {
        RefreshPublicAchievement();
        RefreshUserAchievement();
        localUserDataInfo = AccelByteManager.Instance.AuthLogic.GetUserData();
    }

    /// <summary>
    /// Refresh User Achievement, then set it to temporary user achievement variable first.
    /// so when statistic updated it will gonna compare with the temporary achievement
    /// </summary>
    public void RefreshUserAchievement()
    {
        AccelBytePlugin.GetAchievement().QueryUserAchievements(AchievementSortBy.LISTORDER, result =>
        {
            if (!result.IsError)
            {
                tempUserAchievement = result.Value;
            }
            else
            {
                Debug.Log("[AccelByteAchievementLogic] RefreshUserAchievement(), Error Code: " + result.Error.Code + " | Error Message: " + result.Error.Message);
            }
        });
    }

    /// <summary>
    /// Refresh public achievement, then set it to publicAchievement variable
    /// </summary>
    public void RefreshPublicAchievement()
    {
        AccelBytePlugin.GetAchievement().QueryAchievements("", AchievementSortBy.LISTORDER, result =>
        {
            if (!result.IsError)
            {
                publicAchievement = result.Value;
            }
            else
            {
                Debug.Log("[AccelByteAchievementLogic] RefreshPublicAchievement(), Error Code: " + result.Error.Code + " | Error Message: " + result.Error.Message);
            }
        }
        );
    }

    /// <summary>
    /// First check if Incremental Achievement was called by a correct userId.
    /// because all Client will get all info Statistic Players of the match.
    /// </summary>
    /// <param name="userId">Get a spesific userId to check if owned by client</param>
    public void UnlockIncrementalAchievement(string userId)
    {
        if (userId != null && userId == localUserDataInfo.userId)
        {
            StartCoroutine(UnlockingIncrementalAchievement());
        }
    }

    /// <summary>
    /// Give a delay to wait until server update incremental achievement
    /// </summary>
    /// <returns></returns>
    IEnumerator UnlockingIncrementalAchievement()
    {
        yield return new WaitForSeconds(delayToWaitServerUpdated);
        AccelBytePlugin.GetAchievement().QueryUserAchievements(AchievementSortBy.LISTORDER, result =>
        {
            if (result.IsError)
            {
                Debug.Log("[AccelByteAchievementLogic] CheckingIncrementalAchievement() Error Code: " + result.Error.Code + " Error Message: " + result.Error.Message);
            }
            else
            {
                AchievementPopUpManager achievementPopUpManager = GameObject.FindGameObjectWithTag("PopUpAchievement").GetComponent<AchievementPopUpManager>();
                newUserAchievement = result.Value;
                if (IsAchievedIncrementalAchievement("win-1-match"))
                {
                    achievementPopUpManager.InstantiatePopUpAchievement("win-1-match");
                }
                if (IsAchievedIncrementalAchievement("win-3-match"))
                {
                    achievementPopUpManager.InstantiatePopUpAchievement("win-3-match");
                }
                if (IsAchievedIncrementalAchievement("win-5-match"))
                {
                    achievementPopUpManager.InstantiatePopUpAchievement("win-5-match");
                }
                if (IsAchievedIncrementalAchievement("total-distance-100"))
                {
                    achievementPopUpManager.InstantiatePopUpAchievement("total-distance-100");
                }
                tempUserAchievement = newUserAchievement;
            }
        });
    }

    /// <summary>
    /// This call is to check Non Incremental Achievement
    /// If was not Unlocked yet, then will Unlock Achievement and Instantiate Pop Up Achievement
    /// </summary>
    /// <param name="achievementCode">Get a spesific achievement code</param>
    public void UnlockNonIncrementalAchievement(string achievementCode)
    {
        bool isAchievementAchieved = Array.Exists(tempUserAchievement.data, element => element.achievementCode.Contains(achievementCode));
        if (!isAchievementAchieved)
        {
            AccelBytePlugin.GetAchievement().UnlockAchievement(achievementCode, unlockresult =>
            {
                if (!unlockresult.IsError)
                {
                    AchievementPopUpManager achievementPopUpManager = GameObject.FindGameObjectWithTag("PopUpAchievement").GetComponent<AchievementPopUpManager>();
                    achievementPopUpManager.InstantiatePopUpAchievement(achievementCode);
                }
                else
                {
                    Debug.Log("[AccelByteAchievementLogic] UnlockNonIncrementalAchievement(), Error Code: " + unlockresult.Error.Code + " | Error Message: " + unlockresult.Error.Message);
                }
            });
        }
    }

    /// <summary>
    /// This call is to check if Incremental Achievement was Achieved.
    /// This function is to compare a temporary user achievement with a new user achievement to get true or false return variable
    /// </summary>
    /// <param name="achievementCode">Get a spesific achievement code</param>
    /// <returns>If it is true will Pop Up a Notification</returns>
    private bool IsAchievedIncrementalAchievement(string achievementCode)
    {
        bool tempResult = false;
        UserAchievement tempAchievement = null;
        if (tempUserAchievement != null)
        {
            tempAchievement = Array.Find(tempUserAchievement.data, tempElement => tempElement.achievementCode.Contains(achievementCode));
        }
        UserAchievement newAchievement = Array.Find(newUserAchievement.data, currentElement => currentElement.achievementCode.Contains(achievementCode));
        if (tempAchievement != null)
        {
            if (tempAchievement.status != achievementStatusUnlocked)
            {
                if (newAchievement != null)
                {
                    if (newAchievement.status == achievementStatusUnlocked)
                    {
                        tempResult = true;
                    }
                }
            }
        }
        else
        {
            if (newAchievement != null)
            {
                if (newAchievement.status == achievementStatusUnlocked)
                {
                    tempResult = true;
                }
            }
        }
        Debug.Log("Achievement Code: " + achievementCode + " tempResult: " + tempResult);
        return tempResult;
    }

    /// <summary>
    /// Get a spesific public achievement info
    /// </summary>
    /// <param name="achievementCode"> get a spesific achievement code</param>
    /// <returns></returns>
    public PublicAchievement GetPublicAchievement(string achievementCode)
    {
        return Array.Find(publicAchievement.data, element => element.achievementCode.Contains(achievementCode));
    }

    /// <summary>
    /// Get a spesific user achievement info
    /// </summary>
    /// <param name="achievementCode"> get a spesific achievement code</param>
    /// <returns></returns>
    public UserAchievement GetUserAchievement(string achievementCode)
    {
        return Array.Find(tempUserAchievement.data, element => element.achievementCode.Contains(achievementCode));
    }
}
