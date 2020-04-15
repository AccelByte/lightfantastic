// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

//#define _DEBUG_TWEEN
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TweenComponent : MonoBehaviour
{
    [SerializeField]
    private E_SoundFX selectedSoundFX = E_SoundFX.ButtonClick01;
    /// <summary>
    /// Duration for each tween action
    /// </summary>
    public float AnimationDuration = 0.2f;

    /// <summary>
    /// Persentage value 0-1 of scaling tweening
    /// </summary>
    public float ScalePersentage = 0.9f;

    /// <summary>
    /// Translation move offset
    /// </summary>
    public float TranslationOffset = 20.0f;

    /// <summary>
    /// Scale The gameobject uniformly by AnimateScaleUp
    /// </summary>
    public float ScaleUniformPersentage = 1.5f;

    private bool isJustAnimatedSwipeRight;
    private float localX_OriginalValue;

    private void Awake()
    {
        localX_OriginalValue = gameObject.transform.localPosition.x;
    }
    public void OnButtonClicked()
    {
        // scale to 80 %
        gameObject.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f) * ScalePersentage, AnimationDuration).OnComplete(OnButtonClickedComplete);
        AudioManager.Instance.PlaySoundFX(selectedSoundFX);
    }

    private void OnButtonClickedComplete()
    {
        // scaleback tp 100%
#if _DEBUG_TWEEN
        Debug.Log("TweenComponent OnButtonClickedComplete!");
#endif
        gameObject.transform.DOScale(new Vector3(1f, 1f, 1f), AnimationDuration);
    }

    public void AnimateSwipeRight()
    {
        gameObject.transform.DOLocalMoveX(localX_OriginalValue + TranslationOffset, AnimationDuration);
        AudioManager.Instance.PlaySoundFX(selectedSoundFX);
    }

    public void AnimateSwipeToOriginalLocation()
    {
#if _DEBUG_TWEEN
        Debug.Log("TweenComponent AnimateSwipeLeft!");
#endif
        gameObject.transform.DOLocalMoveX(localX_OriginalValue, AnimationDuration);
        isJustAnimatedSwipeRight = false;
    }

    public void AnimateScaleUp()
    {
        gameObject.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f) * ScaleUniformPersentage, AnimationDuration);
        AudioManager.Instance.PlaySoundFX(selectedSoundFX);
    }

    public void AnimateScaleToNormal()
    {
        // scaleback tp 100%
#if _DEBUG_TWEEN
        Debug.Log("TweenComponent OnAnimateSwipeRight!");
#endif
        gameObject.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), AnimationDuration);
    }

    public void AnimateScaleY_ToZero()
    {
        // scaleback tp 100%
#if _DEBUG_TWEEN
        Debug.Log("TweenComponent OnAnimateSwipeRight!");
#endif
        gameObject.transform.DOScaleY(0.0f, AnimationDuration);
    }

    public void AnimateFadeIn()
    {
#if _DEBUG_TWEEN
        Debug.Log("TweenComponent AnimateFadeIn!");
#endif
        gameObject.transform.GetComponent<CanvasGroup>().DOFade(1, AnimationDuration).OnComplete(AnimateFadeInComplete);
    }

    private void AnimateFadeInComplete()
    {
#if _DEBUG_TWEEN
        Debug.Log("TweenComponent AnimateFadeInComplete!");
#endif
        gameObject.SetActive(true);
    }

    public void AnimateFadeOut()
    {
#if _DEBUG_TWEEN
        Debug.Log("TweenComponent AnimateFadeOut!");
#endif
        gameObject.transform.GetComponent<CanvasGroup>().DOFade(0, AnimationDuration).OnComplete(AnimateFadeOutComplete);
    }

    private void AnimateFadeOutComplete()
    {
#if _DEBUG_TWEEN
        Debug.Log("TweenComponent AnimateFadeOutComplete!");
#endif
        gameObject.SetActive(false);
    }

}