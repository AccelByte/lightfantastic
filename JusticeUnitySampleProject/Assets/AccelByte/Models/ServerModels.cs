// Copyright (c) 2018 - 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Runtime.Serialization;

namespace AccelByte.Models
{
    [DataContract]
    public class RegisterServerRequest
    {
        [DataMember] public string pod_name { get; set; }
        [DataMember] public int port { get; set; }
    }

    [DataContract]
    public class ShutdownServerNotif
    {
        [DataMember] public bool kill_me { get; set; }
        [DataMember] public string pod_name { get; set; }
        [DataMember] public string session_id { get; set; }
    }

    [DataContract]
    public class MatchingAlly
    {
        [DataMember] public string partyId { get; set; }
        [DataMember] public string[] partyMember { get; set; }
    }

    [DataContract]
    public class MatchmakingInfo
    {
        [DataMember] public string sessionId { get; set; }
        [DataMember(Name = "namespace")] public string Namespace { get; set; }
        [DataMember] public string gameMode { get; set; }
        [DataMember] public MatchingAlly[] matchingAllies { get; set; }
        [DataMember] public string podName { get; set; }
    }
}