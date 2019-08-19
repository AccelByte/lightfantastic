using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendPrefab : MonoBehaviour
{
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
