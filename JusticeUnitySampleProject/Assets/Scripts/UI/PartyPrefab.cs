using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyPrefab : MonoBehaviour
{
    [SerializeField]
    private GameObject playerProfile;    

    private Transform playerImage;
    private string partyLeaderID;
    private bool isInitiated;

    private PartyData partyData;

    void Awake()
    {
        partyData = new PartyData();
    }

    private bool GetIsPartyLeader()
    {
        return (partyData.UserID == partyLeaderID);
    }

    public void SetupPlayerProfile(PartyData data, string leaderID)
    {
        playerImage = Instantiate(playerProfile,transform).transform;
        partyLeaderID = leaderID;

        partyData.UserID = data.UserID;
        partyData.PlayerName = data.PlayerName;
        partyData.PlayerEmail = data.PlayerEmail;

        partyData = data;

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
            // show popup profile
            AccelByteManager.Instance.LobbyLogic.ShowPlayerProfile(partyData);
            Debug.Log("PartyPrefab SetupPlayerProfilePopup Popup is shown");
        }
        else
        {
            Debug.Log("PartyPrefab SetupPlayerProfilePopup Popup is not yet setup");
        }
    }

    public void OnKickButtonClicked()
    {
        AccelByteManager.Instance.LobbyLogic.KickPartyMember(partyData.UserID);
        Debug.Log("PartyPrefab OnKickButtonClicked Popup is not yet setup");
    }

    public void OnClearProfileButton()
    {
        if (isInitiated)
        {
            Destroy(playerImage.gameObject);
            partyData.UserID = "";
            partyData.PlayerName = "";
            partyData.PlayerEmail = "";
            partyLeaderID = "";

            isInitiated = false;
        }
    }
}
