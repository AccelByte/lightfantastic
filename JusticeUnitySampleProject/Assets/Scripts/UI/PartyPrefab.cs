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
            // setup the member info
            SetupPlayerProfilePopup();
            // show popup
            AccelByteManager.Instance.LobbyLogic.OnPlayerPartyProfileClicked();
        }
        else
        {
            Debug.Log("SetupPlayerProfilePopup Popup is not yet setup");
        }
    }

    public void SetupPlayerProfilePopup()
    {
        playerNameText.text = displayName;
        playerEmailText.text = emailAddress;

        // TODO: set player profile image


        // TODO: Fix button set for 
        //    party leader :  Leader commands = Onclick for own player profile            :: show his own info, leave and disband party command
        //                                    = Onclick for other member's player profile :: show other player's info and kick command
        //    party member :  Member commands = Onclick for own player profile            :: show his own info, leave party command
        //                                    = Onclick for other member's player profile :: show other player's info

        LeaderCommand.gameObject.SetActive(false);
        MemberCommand.gameObject.SetActive(false);
        if (GetIsPartyLeader())
        {
            //LeaderCommand.gameObject.SetActive(!LeaderCommand.gameObject.activeSelf);
            MemberCommand.gameObject.SetActive(!MemberCommand.gameObject.activeSelf);
        }
        //else
        //{
        //    MemberCommand.gameObject.SetActive(!MemberCommand.gameObject.activeSelf);
        //}
    }
}
