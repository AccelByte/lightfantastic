// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using UnityEngine.Experimental.U2D.Animation;

[RequireComponent(typeof(SpriteResolver))]
public class CharacterHatSetter : MonoBehaviour
{
    [SerializeField] 
    public SpriteResolver spriteResolver;

    public void SetHatSprite(string spriteName)
    {
        spriteResolver.SetCategoryAndLabel(LightFantasticConfig.ItemTags.hat, spriteName);
    }
}
