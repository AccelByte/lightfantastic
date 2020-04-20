// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChatData
{
    public string UserId;
    public List<string> message;
    public List<string> sender;

    public ChatData(string userId, List<string> message, List<string> sender)
    {
        this.UserId = userId;
        this.message = message;
        this.sender = sender;
    }
}
