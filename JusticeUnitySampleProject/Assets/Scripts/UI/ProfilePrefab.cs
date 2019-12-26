using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePrefab : MonoBehaviour
{
    private Transform playerProfileName;

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

    public void SetupProfileUI(string playerProfileName, string playerLevel)
    {
        this.playerProfileName.GetComponent<Text>().text = playerProfileName;
        this.playerLevel.GetComponent<Text>().text = playerLevel;
    }
}
