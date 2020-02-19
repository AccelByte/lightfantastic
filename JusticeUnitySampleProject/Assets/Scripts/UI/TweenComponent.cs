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

    private bool isJustAnimatedSwipeRight;
    private float localX_OriginalValue;

    private void Awake()
    {
        localX_OriginalValue = gameObject.transform.localPosition.x;
    }
    public void OnButtonClicked()
    {
        // scale to 80 %
        gameObject.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f)* ScalePersentage, AnimationDuration).OnComplete(OnButtonClickedComplete);
        AudioManager.Instance.PlaySoundFX(selectedSoundFX);
    }

    private void OnButtonClickedComplete()
    {
        // scaleback tp 100%
        Debug.Log("TweenComponent OnButtonClickedComplete!");
        gameObject.transform.DOScale(new Vector3(1f, 1f, 1f), AnimationDuration);
    }

    public void AnimateSwipeRight()
    {
        gameObject.transform.DOLocalMoveX(gameObject.transform.localPosition.x + TranslationOffset, AnimationDuration);
        AudioManager.Instance.PlaySoundFX(selectedSoundFX);
    }

    public void AnimateSwipeToOriginalLocation()
    {
        Debug.Log("TweenComponent AnimateSwipeLeft!");
        gameObject.transform.DOLocalMoveX(localX_OriginalValue, AnimationDuration);
        isJustAnimatedSwipeRight = false;
    }

    public void AnimateScaleUp()
    {
        gameObject.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f) * 1.5f, AnimationDuration);
        AudioManager.Instance.PlaySoundFX(selectedSoundFX);
    }

    public void AnimateScaleToNormal()
    {
        // scaleback tp 100%
        Debug.Log("TweenComponent OnAnimateSwipeRight!");
        gameObject.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f) , AnimationDuration);
    }
}
