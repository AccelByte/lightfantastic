using AccelByte.Core;
using AccelByte.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendPrefab : MonoBehaviour
{
    private string userId;
    [SerializeField]
    private Text usernameText;
    [SerializeField]
    private Text lastSeenText;
    // Start is called before the first frame update
    public void SetupFriendUI(string username, string lastSeen)
    {
        usernameText.text = username;
        lastSeenText.text = lastSeen;
    }
}
