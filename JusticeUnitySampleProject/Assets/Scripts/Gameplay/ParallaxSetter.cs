// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using UnityEngine;

[Serializable]
class ParallaxComponent
{
    [SerializeField] public MeshRenderer mesh;
    [SerializeField] public float speedModifier;
}

public class ParallaxSetter : MonoBehaviour
{
    [SerializeField] private ParallaxComponent[] parallaxComponents;
    [SerializeField] private float globalParallaxSpeedModifier = 1.0f;
    [SerializeField] private float finalSpeedMultiplier = 1.0f;

    private void Awake()
    {
        foreach (var item in parallaxComponents)
        {
            item.mesh.material.mainTextureOffset.Set(0,0);
        }
    }

    void Update()
    {
        foreach (var item in parallaxComponents)
        {
            var newOffset = new Vector2(item.speedModifier * globalParallaxSpeedModifier * finalSpeedMultiplier, 0.0f);
            var oldOffest = item.mesh.material.mainTextureOffset;
            item.mesh.material.mainTextureOffset = oldOffest + newOffset;
        }
    }

    public void SetSpeed(float speed)
    {
        globalParallaxSpeedModifier = speed; 
    }
}
