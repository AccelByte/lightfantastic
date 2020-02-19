using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TweenComponent : MonoBehaviour
{
    /// <summary>
    /// Duration for each tween action
    /// </summary>
    public float AnimationDuration = 0.2f;

    /// <summary>
    /// Persentage value 0-1 of scaling tweening
    /// </summary>
    public float ScalePersentage = 0.9f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonClicked()
    {
        // scale to 80 %
        gameObject.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f)* ScalePersentage, AnimationDuration).OnComplete(OnButtonClickedComplete);
        AudioManager.Instance.PlaySoundFX(E_SoundFX.ButtonClick01);
    }

    private void OnButtonClickedComplete()
    {
        // scaleback tp 100%
        Debug.Log("TweenComponent OnButtonClickedComplete!");
        gameObject.transform.DOScale(new Vector3(1f, 1f, 1f), AnimationDuration);
    }
}
