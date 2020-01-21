using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerStatusPrefab : MonoBehaviour
{
    [SerializeField]
    private Transform playerNameText;
    [SerializeField]
    private Transform playerIdText;
    [SerializeField]
    private Transform playerEmailText;

    [SerializeField]
    private Transform playerLevelText;
    [SerializeField]
    private Transform playerLevelExpText;

    [SerializeField]
    private Transform playerCollectionHeadGear;
    [SerializeField]
    private Transform playerCollectionTrails;

    private Transform playerAvatar;

    private string playerName;
    private string playerId;
    private string playerEmail;

    private int playerLevel;
    private int playerLevelExp;

    private const string COLLECTION_HEADGEAR = "Head Gear";
    private const string COLLECTION_TRAILS = "Trails";

    void Awake()
    {
        playerLevelText = transform.Find("LevelText");
        playerLevelExpText = transform.Find("PlayCharacterImage");
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateInfo("Light Fan", "0001", "lightfan@accelbyte.net");
    }

    // Update Info
    void UpdateInfo(string name, string id, string email)
    {
        playerName = name;
        playerId = id;
        playerEmail = email;

        playerLevel = 8;
        playerLevelExp = 888;

        playerNameText.GetComponent<TextMeshProUGUI>().text = playerName;
        playerIdText.GetComponent<TextMeshProUGUI>().text = playerId;
        playerEmailText.GetComponent<TextMeshProUGUI>().text = playerEmail;

        // TODO: create player level and exp
        //playerLevelText.GetComponent<TextMeshProUGUI>().text = playerLevel.ToString();
        //playerLevelExpText.GetComponent<TextMeshProUGUI>().text = playerLevelExp.ToString();

        playerCollectionHeadGear.GetComponent<CollectionPrefab>().UpdateInfo(COLLECTION_HEADGEAR, 2, 5);
        playerCollectionTrails.GetComponent<CollectionPrefab>().UpdateInfo(COLLECTION_TRAILS, 1, 3);
    }
}
