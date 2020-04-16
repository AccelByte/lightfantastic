// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;

[RequireComponent(typeof(ParticleSystemRenderer))]
public class CharacterParticleSetter : MonoBehaviour
{
    [SerializeField] 
    public CharacterParticleLibraryAndResolver particleLibraryAndResolver;

    public void SetItem(string item)
    {
        particleLibraryAndResolver.Select(item);
    }
}
