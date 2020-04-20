// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterSpeedSetter : MonoBehaviour
{
    [SerializeField] 
    public Animator animator;

    public void SetSpeed(float speed)
    {
        animator.SetFloat("Speed", speed * LightFantasticConfig.CURR_SPEED_MULTIPLIER_ANIMATION);
    }
}
