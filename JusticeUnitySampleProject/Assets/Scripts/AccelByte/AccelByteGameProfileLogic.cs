using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AccelByte;
using AccelByte.Api;
using AccelByte.Models;
using AccelByte.Core;

/// <summary>
///  GAME PROFILE IS DEPRECATED
///  TODO : Clean up unused objects
/// </summary>
public class AccelByteGameProfileLogic : MonoBehaviour
{
    private GameProfiles abGameProfiles;

    private List<GameProfile> LocalGameProfiles;
    private List<UserGameProfiles> PartyMemberGameProfiles;

    private UserData localPlayerUserData;
    private GameProfile CurrentGameProfile;
    private List<GameProfileAttribute> CurrentGameProfileAttributes;

    // TODO: create struct of player status
    private string playerLevel;
    private string playerHighestScore;

    private const string PLAYER_LEVEL = "playerlevel";
    private const string PLAYER_HIGHEST_SCORE = "playerhighestscore";

    [SerializeField]
    private Transform playerProfilePanel = null;
    private Transform profileUIPrefab;
    private Transform profileListPanel;
    private Transform profileItemScrollView;
    [SerializeField]
    private Transform profileScrollContent;
    [SerializeField]
    private GameObject profileAttributePrefab;
    
    private List<GameObject> profileAttributes;

    void Awake()
    {
        PartyMemberGameProfiles = new List<UserGameProfiles>();
        LocalGameProfiles = new List<GameProfile>();
        CurrentGameProfileAttributes = new List<GameProfileAttribute>();
        profileAttributes = new List<GameObject>();

        SetupUI();
    }

    void SetupUI()
    {
        profileUIPrefab = playerProfilePanel.Find("Profile");
        profileListPanel = playerProfilePanel.Find("ProfileListPanel");
        profileItemScrollView = playerProfilePanel.Find("ItemScrollView");
    }

    void InitGameProfileAttributeUI()
    {
        if (profileScrollContent.childCount > 0)
        {
            ClearGameProfileAttributeUI();
        }

        if (CurrentGameProfileAttributes.Count > 0)
        {
            for (int i = 0; i < CurrentGameProfileAttributes.Count; i++)
            {
                GameObject attribute = Instantiate(this.profileAttributePrefab, Vector3.zero, Quaternion.identity);
                ProfileAttributePrefab attributePrefab = attribute.GetComponent<ProfileAttributePrefab>();

                attributePrefab.SetupProfileAttributeUI(CurrentGameProfileAttributes[i].name, CurrentGameProfileAttributes[i].value);

                attribute.transform.SetParent(profileScrollContent, false);
                profileAttributes.Add(attribute);
            }
        }
    }

    private void ClearGameProfileAttributeUI()
    {
        if (profileAttributes.Count > 0)
        {
            // Clear the party slot buttons
            for (int i = 0; i < profileAttributes.Count; i++)
            {
                profileAttributes[i].GetComponent<ProfileAttributePrefab>().OnClearProfileButton();
            }

            profileAttributes.Clear();

            for (int i = 0; i < profileScrollContent.childCount; i++)
            {
                Destroy(profileScrollContent.GetChild(i).gameObject);
            }
        }
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

    public void DeleteLocalPlayerGameProfile()
    {
        if (CurrentGameProfile != null)
        {
            DeleteGameProfile(CurrentGameProfile.profileId, OnDeleteGameProfile);
        }
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

    public void GetGameProfileAttribute_Level()
    {
        abGameProfiles.GetGameProfileAttribute(CurrentGameProfile.profileId, PLAYER_LEVEL, OnGetGameProfileAttribute);
    }

    public void GetGameProfileAttribute_Score()
    {
        abGameProfiles.GetGameProfileAttribute(CurrentGameProfile.profileId, PLAYER_HIGHEST_SCORE, OnGetGameProfileAttribute);
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

    public void SetupProfileUIContent()
    {
        if (LocalGameProfiles.Count > 0)
        {
            // setup the profile prefab here
            profileUIPrefab.GetComponent<ProfilePrefab>().SetupProfileUI(LocalGameProfiles[0].profileName, playerLevel);
        }

        InitGameProfileAttributeUI();
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

                    // fetching game profile attributes
                    GetGameProfileAttribute_Level();
                    GetGameProfileAttribute_Score();
                }
            }
            else
            {
                // if there is no game profile then make it
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

            GetAllGameProfiles(OnGetAllGameProfiles);
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

    private void OnGetGameProfileAttribute(Result<GameProfileAttribute> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnGetGameProfileAttribute failed:" + result.Error.Message);
            Debug.Log("OnGetGameProfileAttribute Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnGetGameProfileAttribute : " + result.Value.name + " success! Value : " + result.Value.value +"!");
            if (CurrentGameProfileAttributes.Contains(result.Value))
            {
                Debug.Log("OnGetGameProfileAttribute Game profile atribute duplication");
            }
            else
            {
                CurrentGameProfileAttributes.Add(result.Value);
                switch (result.Value.name)
                {
                    case PLAYER_LEVEL:
                        playerLevel = result.Value.value;
                        break;
                    case PLAYER_HIGHEST_SCORE:
                        playerHighestScore = result.Value.value;
                        break;
                    default:
                        break;
                }
            }

            // update UI
            SetupProfileUIContent();
        }
    }

    private GameProfileRequest GetGameProfileRequest()
    {
        GameProfileRequest gameProfileRequest = new GameProfileRequest
        {
            label = "LF_GameProfile",
            profileName = localPlayerUserData.displayName + "_LightFan",
            tags = new string[] { "player", "lightfantastic" },
            attributes = new Dictionary<string, string>()
            {
                {PLAYER_LEVEL,"1"},
                {PLAYER_HIGHEST_SCORE,"0"}
            }
        };

        return gameProfileRequest;
    }
}
