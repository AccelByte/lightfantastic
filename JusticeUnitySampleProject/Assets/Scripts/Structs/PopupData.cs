// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PopupData
{
    public E_PopupType PopupType;

    public string TittleText;

    public string DescriptionText;

    [Header("Primary Button")]
    public string PrimaryButtonText;
    public UnityAction primaryButtonAction;

    [Header("Secondary Button")]
    public string SecondaryButtonText;
    public UnityAction secondaryButtonAction;

    //[HideInInspector]
    //public AudioSource Source;
}
