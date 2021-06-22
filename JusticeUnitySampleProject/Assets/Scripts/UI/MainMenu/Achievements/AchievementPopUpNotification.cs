using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AchievementPopUpNotification : MonoBehaviour
{
    [SerializeField]
    private Image achievementImage;
    [SerializeField]
    private Text achievementName;
    [SerializeField]
    private Text achievementDescription;
    private Texture2D popUpImage;
    private string popUpName;
    private string popUpDescription;
    private float delayToFadeOut = 3f; // default value of delayToFadeOut
    private float durationFadeUI = 1f; // default value of durationFadeUI

    /// <summary>
    /// Instantiate Item of Pop Up Notification
    /// </summary>
    /// <param name="imageUrl"> url to download Achievement Image</param>
    /// <param name="name"> name of Achievement</param>
    /// <param name="description"> description of Achievement</param>
    public void InstantiatePopUp(string imageUrl, string name, string description)
    {
        popUpName = name;
        popUpDescription = description;
        StartCoroutine(DownloadImage(imageUrl));
    }

    /// <summary>
    /// Download and request an image of achievement
    /// </summary>
    /// <param name="url"> url to download image of achievement</param>
    /// <returns></returns>
    IEnumerator DownloadImage(string url)
    {
        UnityWebRequest requestTexture = UnityWebRequestTexture.GetTexture(url);
        yield return requestTexture.SendWebRequest();

        if (requestTexture.isNetworkError || requestTexture.isHttpError)
        {
            Debug.Log("[AchievementPopUpNotification] DownloadImage(), Error: " + requestTexture.error);
        }
        else
        {
            popUpImage = ((DownloadHandlerTexture)requestTexture.downloadHandler).texture;
            InstantiatePopUpInfo();
        }
    }

    /// <summary>
    /// Instantiate Pop Up info then start to Fade the UI
    /// </summary>
    private void InstantiatePopUpInfo()
    {
        achievementImage.sprite = Sprite.Create(popUpImage, new Rect(0, 0, popUpImage.width, popUpImage.height), new Vector2(0, 0));
        achievementName.text = popUpName;
        achievementDescription.text = popUpDescription;
        StartCoroutine(FadeUI(true));
    }

    /// <summary>
    /// Fading animate UI
    /// </summary>
    /// <param name="isFadeIn"> is Fade In or Fade Out</param>
    /// <returns> </returns>
    IEnumerator FadeUI(bool isFadeIn)
    {
        float startAlpha;
        float endAlpha;
        float counter = 0f; // set default counter to count the time
        CanvasGroup canvasGroup = this.gameObject.GetComponent<CanvasGroup>();
        if (isFadeIn)
        {
            startAlpha = 0;
            endAlpha = 1;
        }
        else
        {
            startAlpha = 1;
            endAlpha = 0;
        }

        while (counter < durationFadeUI)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, counter / durationFadeUI);
            if (counter >= durationFadeUI)
            {
                if (isFadeIn)
                {
                    StartCoroutine(WaitForSecondsToFadeOut(delayToFadeOut));
                }
                else
                {
                    Destroy(this.gameObject);
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Wait a second then Pop Up will disappear
    /// </summary>
    /// <param name="duration"> Num Seconds to Wait</param>
    /// <returns></returns>
    IEnumerator WaitForSecondsToFadeOut(float duration)
    {
        yield return new WaitForSeconds(duration);
        StartCoroutine(FadeUI(false));
    }
}
