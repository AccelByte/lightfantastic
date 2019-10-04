using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyPrefab : MonoBehaviour
{
    [SerializeField]
    private Text usernameText;
    [SerializeField]
    private Text lastSeenText;

    public void SetupPartyUI(string username,string lastSeen)
    {
        usernameText.text = username;
        lastSeenText.text = lastSeen;
    }
}
