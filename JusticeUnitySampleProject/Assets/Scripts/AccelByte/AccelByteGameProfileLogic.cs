using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AccelByte;
using AccelByte.Api;
using AccelByte.Models;
using AccelByte.Core;

public struct LightFantasticAttibute
{
    string playerlevel;
}

public class AccelByteGameProfileLogic : MonoBehaviour
{
    private GameProfiles abGameProfiles;

    private List<GameProfile> LocalGameProfiles;
    private List<UserGameProfiles> PartyMemberGameProfiles;

    private UserData localPlayerUserData;
    private GameProfile CurrentGameProfile;

    private const string PLAYER_LEVEL = "playerlevel";
    private const string PLAYER_HIGHEST_SCORE = "playerhigestscore";

    void Awake()
    {
        PartyMemberGameProfiles = new List<UserGameProfiles>();
        LocalGameProfiles = new List<GameProfile>();
    }
    // Start is called before the first frame update
    void Start()
    {
        abGameProfiles = AccelBytePlugin.GetGameProfiles();
    }

    public void SetupGameProfile()
    {
        localPlayerUserData = AccelByteManager.Instance.AuthLogic.GetUserData();

        // get all game profiles
        GetAllGameProfiles(OnGetAllGameProfiles);
    }

    public void GetAllPartyMemberGameProfile()
    {
        List<PartyData> partyDatas = AccelByteManager.Instance.LobbyLogic.GetMemberPartyData();

        string[] partyUserIds =  new string[partyDatas.Count];
        for (int i = 0; i < partyUserIds.Length; i++)
        {
            partyUserIds[i] = partyDatas[i].UserID;
        }

        BatchGetGameProfiles(partyUserIds, OnBatchGetGameProfiles);
    }

    public GameProfile GetCurrentGameProfile()
    {
        return CurrentGameProfile;
    }

    public void BatchGetGameProfiles(string[] userIds, ResultCallback<UserGameProfiles[]> callback)
    {
        abGameProfiles.BatchGetGameProfiles(userIds, callback);
    }

    public void GetAllGameProfiles(ResultCallback<GameProfile[]> callback)
    {
        abGameProfiles.GetAllGameProfiles(callback);
    }

    public void GetGameProfile(string profileId, ResultCallback<GameProfile> callback)
    {
        abGameProfiles.GetGameProfile(profileId, callback);
    }

    public void CreateGameProfile(GameProfileRequest gameProfileRequest, ResultCallback<GameProfile> callback)
    {
        abGameProfiles.CreateGameProfile(gameProfileRequest, callback);
    }

    public void UpdateGameProfile(GameProfile gameProfile, ResultCallback<GameProfile> callback)
    {
        abGameProfiles.UpdateGameProfile(gameProfile, callback);
    }

    public void DeleteGameProfile(string profileId, ResultCallback callback)
    {
        abGameProfiles.DeleteGameProfile(profileId, callback);
    }

    public void UpdateGameProfile_PlayerLevel(string profileId, int playerLevel)
    {
        GameProfileAttribute attribute = new GameProfileAttribute();
        attribute.name = PLAYER_LEVEL;
        attribute.value = playerLevel.ToString();
        abGameProfiles.UpdateGameProfileAttribute(profileId, attribute, OnUpdateGameProfile);
    }

    public void UpdateGameProfile_PlayerHighestScore(string profileId, int playerHighestScore)
    {
        GameProfileAttribute attribute = new GameProfileAttribute();
        attribute.name = PLAYER_HIGHEST_SCORE;
        attribute.value = playerHighestScore.ToString();
        abGameProfiles.UpdateGameProfileAttribute(profileId, attribute, OnUpdateGameProfile);
    }

    private void OnBatchGetGameProfiles(Result<UserGameProfiles[]> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnBatchGetGameProfile failed:" + result.Error.Message);
            Debug.Log("OnBatchGetGameProfile Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnBatchGetGameProfile success!");
            if (result.Value.Length > 0)
            {
                Debug.Log("OnBatchGetGameProfile UserGameProfiles found : " + result.Value.Length);

                for (int i = 0; i < result.Value.Length; i++)
                {
                    PartyMemberGameProfiles.Add(result.Value[i]);
                    Debug.Log("OnBatchGetGameProfile UserGameProfiles found userid : " + result.Value[i].userId);
                }
            }
        }
    }

    private void OnGetAllGameProfiles(Result<GameProfile[]> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnGetAllGameProfiles failed:" + result.Error.Message);
            Debug.Log("OnGetAllGameProfiles Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnGetAllGameProfiles success!");
            if (result.Value.Length > 0)
            {
                Debug.Log("OnGetAllGameProfiles Gameprofiles found : " + result.Value.Length);

                for (int i = 0; i < result.Value.Length; i++)
                {
                    LocalGameProfiles.Add(result.Value[i]);
                    Debug.Log("OnGetAllGameProfiles Gameprofiles found profileId: " + result.Value[i].profileId);
                }

                // choose default gameprofile 0 as current
                if (LocalGameProfiles.Count > 0)
                {
                    CurrentGameProfile = LocalGameProfiles[0];
                }
            }
            else
            {
                // if there is no game profiel then make onw
                CreateGameProfile(GetGameProfileRequest(), OnCreateGameProfile);
            }
        }
    }

    private void OnGetGameProfile(Result<GameProfile> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnGetGameProfile failed:" + result.Error.Message);
            Debug.Log("OnGetGameProfile Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnGetGameProfile success!");
            Debug.Log("OnGetGameProfile Gameprofile found profile ID: " + result.Value.profileId);
        }
    }

    private void OnCreateGameProfile(Result<GameProfile> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnCreateGameProfile failed:" + result.Error.Message);
            Debug.Log("OnCreateGameProfile Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnCreateGameProfile success!");
            Debug.Log("OnCreateGameProfile Gameprofile found profile ID: " + result.Value.profileId);
        }
    }

    private void OnUpdateGameProfile(Result<GameProfile> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnUpdateGameProfile failed:" + result.Error.Message);
            Debug.Log("OnUpdateGameProfile Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnUpdateGameProfile success!");
            Debug.Log("OnUpdateGameProfile Gameprofile found profile ID: " + result.Value.profileId);
        }
    }

    private void OnDeleteGameProfile(Result result)
    {
        if (result.IsError)
        {
            Debug.Log("OnUpdateGameProfile failed:" + result.Error.Message);
            Debug.Log("OnUpdateGameProfile Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnUpdateGameProfile success!");
            Debug.Log("OnUpdateGameProfile Gameprofile deleted");
        }
    }

    private GameProfileRequest GetGameProfileRequest()
    {
        GameProfileRequest gameProfileRequest = new GameProfileRequest();

        gameProfileRequest.label = "LF_GameProfile";
        gameProfileRequest.profileName = localPlayerUserData.displayName + "_LightFan";
        gameProfileRequest.tags = new string[] {"player", "lightfantastic"};
        gameProfileRequest.attributes = new Dictionary<string, string>()
        {
            {PLAYER_LEVEL,"1"},
            {PLAYER_HIGHEST_SCORE,"0"}
        };

        return gameProfileRequest;
    }
}
