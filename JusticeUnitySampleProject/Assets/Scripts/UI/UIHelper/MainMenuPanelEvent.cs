using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPanelEvent : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log("MainMenu");
        AccelByteManager.Instance.AchievementLogic.RefreshAchievement();
    }
}
