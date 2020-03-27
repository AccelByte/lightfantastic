using ABRuntimeLogic;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(AccelByteAuthenticationLogic))]
[RequireComponent(typeof(AccelByteLobbyLogic))]
[RequireComponent(typeof(AccelByteWalletLogic))]
[RequireComponent(typeof(AccelByteGameProfileLogic))]
[RequireComponent(typeof(AccelByteUserProfileLogic))]
[RequireComponent(typeof(AccelByteStatisticLogic))]
[RequireComponent(typeof(AccelByteServerLogic))]
[RequireComponent(typeof(AccelByteEntitlementLogic))]
[RequireComponent(typeof(MultiplayerMenu))]
public class AccelByteManager : MonoBehaviour
{
    private static AccelByteManager instance;
    public static AccelByteManager Instance { get { return instance; } }

    private AccelByteAuthenticationLogic authLogic;
    public AccelByteAuthenticationLogic AuthLogic { get { return authLogic; } }
    private AccelByteLobbyLogic lobbyLogic;
    public AccelByteLobbyLogic LobbyLogic { get { return lobbyLogic; } }
    private AccelByteWalletLogic walletLogic;
    public AccelByteWalletLogic WalletLogic { get { return walletLogic; } }

    private AccelByteGameProfileLogic gameprofileLogic;
    public AccelByteGameProfileLogic GameProfileLogic { get { return gameprofileLogic; } }
    private AccelByteUserProfileLogic userProfileLogic;
    public AccelByteUserProfileLogic UserProfileLogic { get { return userProfileLogic; } }
    private AccelByteStatisticLogic userStaticticLogic;
    public AccelByteStatisticLogic UserStaticticLogic { get { return userStaticticLogic; } }
    private AccelByteEntitlementLogic entitlementLogic;
    public AccelByteEntitlementLogic EntitlementLogic { get { return entitlementLogic; } }
    private MultiplayerMenu multiplayerLogic;

    [Header("Server Logic")]
    [SerializeField][Tooltip("WARNING! Don't remove this prefab.")]
    private AccelByteServerLogic serverLogicPrefab = null;
    [SerializeField]
    private string localDSName = "LocalTestDS";
    [SerializeField][Tooltip("Will be ignored on standalone mode. Only work in editor")]
    private bool asLocalDS = true;
    public string LocalDSName { get { return localDSName; } }

    public AccelByteServerLogic ServerLogic { get { return serverLogic; } }
    private AccelByteServerLogic serverLogic;

    public CountryObject[] countryObjectsCache = null;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        authLogic = gameObject.GetComponent<AccelByteAuthenticationLogic>();
        lobbyLogic = gameObject.GetComponent<AccelByteLobbyLogic>();
        walletLogic = gameObject.GetComponent<AccelByteWalletLogic>();
        gameprofileLogic = gameObject.GetComponent<AccelByteGameProfileLogic>();
        userProfileLogic = gameObject.GetComponent<AccelByteUserProfileLogic>();
        userStaticticLogic = gameObject.GetComponent<AccelByteStatisticLogic>();
        multiplayerLogic = gameObject.GetComponent<MultiplayerMenu>();
        entitlementLogic = gameObject.GetComponent<AccelByteEntitlementLogic>();
        MainThreadTaskRunner.CreateGameObject();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (
        #if UNITY_EDITOR
            EditorUserBuildSettings.enableHeadlessMode    // If "BuildSetting">"ServerBuild" has a checkmark
        #else
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null // If it's a server that doesn't have graphic
        #endif
            )  
        {
            serverLogic = Instantiate(serverLogicPrefab, Vector3.zero, Quaternion.identity);
            serverLogic.onServerRegistered += multiplayerLogic.Host;
            serverLogic.LocalDSName = localDSName;
            serverLogic.isLocal = asLocalDS;
        }
        else
        {
            Debug.Log("This instance will become a client");
        }
        
        DeveloperConsoleHelper.Instance.Refresh();
    }
}
