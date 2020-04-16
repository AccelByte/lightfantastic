// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

//Disables the warning messages generated from private [SerializeField]
#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AccelByte.Api;
using AccelByte.Models;
using AccelByte.Core;

public class AccelByteWalletLogic : MonoBehaviour
{
    private Wallet abUserWallet;

    #region Main Menu Currency UI
    private Text ionValue;
    private Text photonValue;
    #endregion

    // Start is called before the first frame update
    void OnEnable()
    {
        abUserWallet = AccelBytePlugin.GetWallet();

        Debug.Log(abUserWallet);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
