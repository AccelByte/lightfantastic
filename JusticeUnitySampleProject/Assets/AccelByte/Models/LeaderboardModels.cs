// Copyright (c) 2018 - 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Runtime.Serialization;

namespace AccelByte.Models
{
    [DataContract]
    public class UserPoint
    {
        [DataMember] public float point { get; set; }
        [DataMember] public string userId { get; set; }
    }

    [DataContract]
    public class UserRanking
    {
        [DataMember] public float point { get; set; }
        [DataMember] public int rank { get; set; }
    }

    [DataContract]
    public class UserRankingData
    {
        [DataMember] public UserRanking allTime { get; set; }
        [DataMember] public UserRanking current { get; set; }
        [DataMember] public UserRanking daily { get; set; }
        [DataMember] public UserRanking monthly { get; set; }
        [DataMember] public string userId { get; set; }
        [DataMember] public UserRanking weekly { get; set; }
    }

    [DataContract]
    public class LeaderboardRankingResult
    {
        [DataMember] public UserPoint[] data { get; set; }
        [DataMember] public Paging paging { get; set; }
    }
}
