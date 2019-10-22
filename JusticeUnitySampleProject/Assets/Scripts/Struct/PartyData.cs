using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PartyData
{
    public string UserID;
    public string PlayerName;
    public string PlayerEmail;

    public PartyData(string userId, string playerName, string playerEmail)
    {
        this.UserID = userId;
        this.PlayerName = playerName;
        this.PlayerEmail = playerEmail;
    }
}
