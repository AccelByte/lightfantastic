// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SoundFX
{
    public E_SoundFX Name;
    public AudioClip Clip;

    [Range(0f,1f)]
    public float Volume;
    [Range(0.1f, 3f)]
    public float Pitch;

    public bool IsLooping;

    [HideInInspector]
    public AudioSource Source;
}
