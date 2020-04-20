// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePrefab : MonoBehaviour
{
    [SerializeField]
    private Transform playerProfileName;
    [SerializeField]
    private Transform playerLevel;

    private Transform playerAvatar;

    void Awake()
    {
        playerProfileName = transform.Find("ProfileNameText");
        playerLevel = transform.Find("LevelText");
        playerAvatar = transform.Find("PlayCharacterImage");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetupProfileUI(string profileName, string level)
    {
        this.playerProfileName.GetComponent<Text>().text = profileName;
        this.playerLevel.GetComponent<Text>().text = "lvl " + level;
    }
}
