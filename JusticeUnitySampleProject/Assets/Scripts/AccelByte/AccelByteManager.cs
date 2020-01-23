﻿using ABRuntimeLogic;
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
    private AccelByteUserProfileLogic userProfileLogic;
    public AccelByteUserProfileLogic UserProfileLogic { get { return userProfileLogic; } }
    private AccelByteStatisticLogic userStaticticLogic;
    public AccelByteStatisticLogic UserStaticticLogic { get { return userStaticticLogic; } }

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
    }
}
