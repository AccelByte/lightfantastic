using AccelByte.Api;
using AccelByte.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AchievementPopUpManager : MonoBehaviour
{
    [SerializeField]
    private GridLayoutGroup gridLayoutGroup;
    [SerializeField]
    private GameObject achievementPopUpNotification;

    /// <summary>
    /// Instantiate new Pop Up Achievement then parent it in gridLayoutGroup
    /// </summary>
    /// <param name="achievementCode"> get a spesific Achievement code to get a valid info of achievement</param>
    public void InstantiatePopUpAchievement(string achievementCode)
    {
        PublicAchievement achievement = AccelByteManager.Instance.AchievementLogic.GetPublicAchievement(achievementCode);
        if (achievement != null)
        {
            GameObject achievementPopUp = Instantiate(achievementPopUpNotification, gridLayoutGroup.transform);
            achievementPopUp.GetComponent<AchievementPopUpNotification>().InstantiatePopUp(achievement.unlockedIcons[0].url, achievement.name, achievement.description);
        }
    }
}
