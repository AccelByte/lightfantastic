using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementPrefab : MonoBehaviour
{
    //Prefab for item achievement
    [SerializeField]
    private GameObject incrementalAchivement;
    [SerializeField]
    private GameObject nonIncrementalAchivement;
    private int indexOfAchievementIcon = 0; // This index to spesify icon index to Download the correct index of Achievement Icon
    
    private enum AchievementUnlockedVariant // to spesifcy variant of Achievement Status Unlocked or Locked
    {
        COMPLETED = 2,
        PROGRESS = 1
    }

    //AnchorList
    [SerializeField]
    private Transform gridList;

    /// <summary>
    /// Refresh New Achievement after AchievementPrefab enable or active
    /// </summary>
    private void OnEnable()
    {
        StartCoroutine(RequestNewAchievement());
    }

    /// <summary>
    /// Removing and add new Achievement List
    /// </summary>
    /// <returns> Wait until all Achievement List Remove then will Request a new Achievement</returns>
    IEnumerator RequestNewAchievement()
    {
        RemoveAchievementList();
        yield return new WaitForEndOfFrame(); // Wait until all child of frame is Removed
        AddAchievementList();
    }

    /// <summary>
    /// Remove all list of Achievement at current panel Achievement
    /// </summary>
    void RemoveAchievementList()
    {
        foreach (Transform child in gridList.transform)
        {
            Destroy(child.gameObject);
        };
    }

    /// <summary>
    /// Add new list of Achievement and add to grid list panel Achievement
    /// </summary>
    void AddAchievementList()
    {
        // info of Dictionary {achievementCodeTopublicAchievementLookUp}
        Dictionary<string, PublicAchievement> achievementUnlocked = new Dictionary<string, PublicAchievement>(); // to List any of Achievement was Unlocked. When Hidden or Locked Achievement is Initiate, its not gonna Initiate Unlock Achievement again.
        Dictionary<string, PublicAchievement> achievementProgressed = new Dictionary<string, PublicAchievement>(); // to List any of Achievement is Progressed
        Dictionary<string, PublicAchievement> achievementLocked = new Dictionary<string, PublicAchievement>(); // to List any of Achievement is Locked
        Dictionary<string, PublicAchievement> achievementHidden = new Dictionary<string, PublicAchievement>(); // to List any of Achievement is Hidden
        if (AccelByteManager.Instance.AchievementLogic.tempUserAchievement.data != null ||
            AccelByteManager.Instance.AchievementLogic.tempUserAchievement.data.Length != 0)
        {
            var userAchievements = AccelByteManager.Instance.AchievementLogic.tempUserAchievement.data;
            var publicAchievements = AccelByteManager.Instance.AchievementLogic.publicAchievement.data;
            foreach (var publicAchievement in publicAchievements)
            {
                UserAchievement userAchievement = Array.Find(userAchievements, element => element.achievementCode.Contains(publicAchievement.achievementCode));
                if (userAchievement != null)
                {
                    switch (userAchievement.status)
                    {
                        case (int)AchievementUnlockedVariant.COMPLETED:
                            // Add to Unlocked Achievements List
                            achievementUnlocked.Add(userAchievement.achievementCode, publicAchievement);
                            break;
                        case (int)AchievementUnlockedVariant.PROGRESS:
                            // Add to Progress Achievements List
                            achievementProgressed.Add(userAchievement.achievementCode, publicAchievement);
                            break;
                    }
                }
                else
                {
                    switch (publicAchievement.hidden)
                    {
                        case false:
                            // Add to Locked Achievement List
                            achievementLocked.Add(publicAchievement.achievementCode, publicAchievement);
                            break;
                        case true:
                            // Add to Hidden Achievement List
                            achievementHidden.Add(publicAchievement.achievementCode, publicAchievement);
                            break;
                    }
                }
            }

            // Sort Achievement List Item from Unlocked, Progress, Locked, and Hidden.
            if (achievementUnlocked.Count != 0) InstantiateListOfAchievementItem(achievementUnlocked);
            if (achievementProgressed.Count != 0) InstantiateListOfAchievementItem(achievementProgressed);
            if (achievementLocked.Count != 0) InstantiateListOfAchievementItem(achievementLocked);
            if (achievementHidden.Count != 0) InstantiateListOfAchievementItem(achievementHidden);
        }
    }

    /// <summary>
    /// Instantiate every Achievement List Item
    /// This call purposes is to Sort List of Achievement Item
    /// </summary>
    /// <param name="achievementInfo"> achievement info Dictionary {achievementCodeTopublicAchievementLookUp}</param>
    private void InstantiateListOfAchievementItem(Dictionary<string, PublicAchievement> achievementInfo)
    {
        foreach(KeyValuePair<string, PublicAchievement> achievement in achievementInfo)
        {
            UserAchievement userAchievement = AccelByteManager.Instance.AchievementLogic.GetUserAchievement(achievement.Key);
            if (userAchievement != null)
            {
                switch (userAchievement.status)
                {
                    case (int)AchievementUnlockedVariant.COMPLETED:
                        InstantiateAchievementItem(nonIncrementalAchivement, achievement.Value.name, achievement.Value.description, achievement.Value.unlockedIcons[indexOfAchievementIcon].url);
                        break;
                    case (int)AchievementUnlockedVariant.PROGRESS:
                        InstantiateAchievementItem(incrementalAchivement, achievement.Value.name, achievement.Value.description, achievement.Value.unlockedIcons[indexOfAchievementIcon].url, userAchievement.latestValue, achievement.Value.goalValue);
                        break;
                }
            }
            else
            {
                switch (achievement.Value.hidden)
                {
                    case false:
                        // 0 means. this achievement is not a progress achieve yet
                        InstantiateAchievementItem(nonIncrementalAchivement, achievement.Value.name, achievement.Value.description, achievement.Value.lockedIcons[indexOfAchievementIcon].url, 0, achievement.Value.goalValue, achievement.Value.hidden);
                        break;
                    case true:
                        // 0 means. this achievement is not a progress achieve yet
                        InstantiateAchievementItem(nonIncrementalAchivement, achievement.Value.name, achievement.Value.description, achievement.Value.lockedIcons[indexOfAchievementIcon].url, 0, achievement.Value.goalValue, achievement.Value.hidden);
                        break;
                }
            }
        }
    }



    /// <summary>
    /// Instantiate new Achievement item
    /// </summary>
    /// <param name="myPrefab"> spesific prefab that have to set to panel</param>
    /// <param name="title"> title of Achievement</param>
    /// <param name="description"> description of Achievement</param>
    /// <param name="urlIcon"> urlIcon of Achievement</param>
    /// <param name="currentValue"> current progress value of Achievement</param>
    /// <param name="goalValue"> goal progress value of Achievement</param>
    /// <param name="isHidden"> is Achievement hidden</param>
    void InstantiateAchievementItem(GameObject myPrefab, string title, string description, string urlIcon,
        double currentValue = 0, int goalValue=0, bool isHidden=false)
    {
        // Instantiate at position grid list.
        var myAchievement = Instantiate(myPrefab, gridList.position, Quaternion.identity);
        // Reparent Achievement item to GridList transform
        myAchievement.transform.SetParent(gridList.transform, false);
        myAchievement.GetComponent<AchievementItem>().InstantiateAchievementItem(urlIcon, title, description,
            currentValue, goalValue,isHidden);
    }
}
