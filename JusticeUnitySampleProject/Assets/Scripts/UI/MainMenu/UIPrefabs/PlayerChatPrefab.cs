#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;

public class PlayerChatPrefab : MonoBehaviour
{
    public Button activePlayerButton;
    public GameObject backgroundImage;
    public Text usernameText;
    public Image iconPartyLeaderImage;
    public Text partyLeaderUsernameText;


    private string userId;

    public void SetupPlayerChatUI(string username, string userId, bool isOnline = false)
    {
        this.userId = userId;
        usernameText.text = username;
        SetPlayerStatus(isOnline);
    }

    private void SetPlayerStatus(bool isOnline)
    {
        if (isOnline)
        {
            usernameText.color = Color.white; 
        }
        else
        {
            usernameText.color = new Color(0.341f, 0.341f, 0.341f, 1.0f);
        }
    }

    public void SetupPartyLeader(string username)
    {
        usernameText.gameObject.SetActive(false);
        iconPartyLeaderImage.gameObject.SetActive(true);
        partyLeaderUsernameText.text = username;
    }
}
