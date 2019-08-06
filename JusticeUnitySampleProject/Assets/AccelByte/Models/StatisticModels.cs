// Copyright (c) 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AccelByte.Models
{
    public enum StatisticSetBy
    {
        CLIENT,
        SERVER
    }

    public enum StatisticStatus
    {
        INIT,
        TIED
    }

    public enum StatisticType
    {
        INT,
        FLOAT
    }

    [DataContract]
    public class StatInfo
    {
        [DataMember] public string createdAt { get; set; }
        [DataMember] public float defaultValue { get; set; }
        [DataMember] public string description { get; set; }
        [DataMember] public bool incrementOnly { get; set; }
        [DataMember] public float maximum { get; set; }
        [DataMember] public float minimum { get; set; }
        [DataMember] public string name { get; set; }
        [DataMember(Name = "namespace")] public string Namespace { get; set; }
        [DataMember] public bool setAsGlobal { get; set; }
        [DataMember] public StatisticSetBy setBy { get; set; }
        [DataMember] public string statCode { get; set; }
        [DataMember] StatisticStatus status { get; set; }
        [DataMember] StatisticType type { get; set; }
        [DataMember] string updatedAt { get; set; }
    }

    [DataContract]
    public class StatItemInfo
    {
        [DataMember] public string createdAt { get; set; }
        [DataMember(Name = "namespace")] public string Namespace { get; set; }
        [DataMember] string profileId { get; set; }
        [DataMember] string statCode { get; set; }
        [DataMember] string statName { get; set; }
        [DataMember] string updatedAt { get; set; }
        [DataMember] float value { get; set; }
    }

    [DataContract]
    public class StatItemIncResult
    {
        [DataMember] float currentValue { get; set; }
    }

    [DataContract]
    public class StatItemPagingSlicedResult
    {
        [DataMember] StatItemInfo[] data { get; set; }
        [DataMember] Paging paging { get; set; }
    }
}