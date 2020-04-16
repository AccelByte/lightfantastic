// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

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
