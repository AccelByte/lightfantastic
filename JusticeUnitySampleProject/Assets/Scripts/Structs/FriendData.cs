using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FriendData
{
    public string UserId;
    public string DisplayName;
    public DateTime LastSeen;
    public string IsOnline;

    public FriendData(string userId, string displayName, DateTime lastSeen, string isOnline)
    {
        this.UserId = userId;
        this.DisplayName = displayName;
        this.LastSeen = lastSeen;
        this.IsOnline = isOnline;
    }
}
