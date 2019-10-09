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

    public void SetupPlayerProfile(string id, string name, string email)
    {
        playerImage = Instantiate(playerProfile,transform).transform;
        userID = id;
        displayName = name;
        emailAddress = email;
    }
}
