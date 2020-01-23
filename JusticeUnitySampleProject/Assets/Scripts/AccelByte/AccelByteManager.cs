using ABRuntimeLogic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private AccelByteGameProfileLogic userProfileLogic;
    public AccelByteGameProfileLogic UserProfileLogic { get { return userProfileLogic; } }
    private MultiplayerMenu multiplayerLogic;

    [Header("Server Logic")]
    [SerializeField]
    private AccelByteServerLogic serverLogicPrefab = null;
    [SerializeField]
    private string localDSName = "LocalTestDS";
    public string LocalDSName { get { return localDSName; } }

    public AccelByteServerLogic ServerLogic { get { return serverLogic; } }
    private AccelByteServerLogic serverLogic;

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
        multiplayerLogic = gameObject.GetComponent<MultiplayerMenu>();
        MainThreadTaskRunner.CreateGameObject();
    }

    private void Start()
    {
        if (serverLogicPrefab != null)
        {
            serverLogic = Instantiate(serverLogicPrefab, Vector3.zero, Quaternion.identity);
            serverLogic.onServerRegistered += multiplayerLogic.Host;
            serverLogic.LocalDSName = localDSName;
        }
        else
        {
            Debug.Log("ServerLogicPrefab is null, this instance will become a client");
        }
    }
}
