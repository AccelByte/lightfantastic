using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyPrefab : MonoBehaviour
{
    [SerializeField]
    private GameObject playerProfile;    

    [SerializeField]
    private Text playerNameText;
    [SerializeField]
    private Text playerEmailText;
    [SerializeField]
    private Image playerProfileImage;
    [SerializeField]
    private Transform LeaderCommand;
    [SerializeField]
    private Transform MemberCommand;

    private Transform playerImage;
    private string userID;
    private string displayName;
    private string emailAddress;
    private string partyLeaderID;
    private bool isInitiated;
    private bool isPartyLeader;

    private bool GetIsPartyLeader()
    {
        return (userID == partyLeaderID);
    }

    public void SetupPlayerProfile(string id, string name, string email, string leaderID)
    {
        playerImage = Instantiate(playerProfile,transform).transform;
        userID = id;
        displayName = name;
        emailAddress = email;
        partyLeaderID = leaderID;

        if (GetIsPartyLeader())
        {
            playerImage.GetComponent<Image>().color = Color.magenta;
        }
        else
        {
            playerImage.GetComponent<Image>().color = Color.white;
        }

        isInitiated = true;
    }

    public void OnProfileButtonClicked()
    {
        if (isInitiated)
        {
            // show popup
            AccelByteManager.Instance.LobbyLogic.ShowPlayerProfile(displayName);
        }
        else
        {
            Debug.Log("SetupPlayerProfilePopup Popup is not yet setup");
        }
    }
}
