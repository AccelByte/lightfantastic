using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyPrefab : MonoBehaviour
{
    [SerializeField]
    private GameObject playerProfile;    

    private Transform playerImage;
    private string userID;
    private string displayName;
    private string emailAddress;
    private string partyLeaderID;
    private bool isInitiated;

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
            // show popup profile
            AccelByteManager.Instance.LobbyLogic.ShowPlayerProfile(displayName, false, userID);
            Debug.Log("PartyPrefab SetupPlayerProfilePopup Popup is shown");
        }
        else
        {
            Debug.Log("PartyPrefab SetupPlayerProfilePopup Popup is not yet setup");
        }
    }

    public void OnKickButtonClicked()
    {
        AccelByteManager.Instance.LobbyLogic.KickPartyMember(userID);
        Debug.Log("PartyPrefab OnKickButtonClicked Popup is not yet setup");
    }

    public void OnClearProfileButton()
    {
        if (isInitiated)
        {
            Destroy(playerImage.gameObject);
            userID = "";
            displayName = "";
            emailAddress = "";
            partyLeaderID = "";

            isInitiated = false;
        }
    }
}
