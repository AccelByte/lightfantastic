// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using UnityEngine.Experimental.U2D.Animation;

[RequireComponent(typeof(SpriteResolver))]
public class CharacterPlatformSpriteSetter : MonoBehaviour
{
    [SerializeField]
    private SpriteResolver resolver;

    public void SetSprite(LightFantasticConfig.Platform platform)
    {
        resolver.SetCategoryAndLabel(LightFantasticConfig.PLATFORM_LIBRARY_ASSET_CATEGORY, platform.ToString());
    }
}
