using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AchievementItem : MonoBehaviour
{
    [SerializeField]
    private Image icon;
    [SerializeField]
    private Text title;
    [SerializeField]
    private Text text;
    [SerializeField]
    private Slider sliderProgress;

    /// <summary>
    /// This Function is to Instantiate any Item of Achievement to appear at Achievement Panel in User Profile
    /// </summary>
    /// <param name="urlIcon"> url to download achievement image</param>
    /// <param name="title"> title of Achievement</param>
    /// <param name="text"> description text of Achievement</param>
    /// <param name="currentValue"> current value of Achievement to start of</param>
    /// <param name="goalValue"> latest value of Achievement to end of</param>
    /// <param name="isHidden"> is Achievement a Hidden Achievement</param>
    public void InstantiateAchievementItem(string urlIcon="", string _title = "Hidden Achievement"
        ,string _text= "Keep playing to find out", double currentValue=0, int goalValue = 0 ,bool isHidden = false)
    {
        if (!isHidden)
        {
            StartCoroutine(DownloadImage(urlIcon));
            title.text = _title;
            text.text = _text;
        }
        else
        {
            Color tempColor = icon.color;
            tempColor.a = 1;
            icon.color = tempColor;
        }
        if (sliderProgress != null)
        {
            sliderProgress.maxValue = goalValue;
            sliderProgress.value = (float) currentValue;
        }
    }

    /// <summary>
    /// Download Achievement Image with a spesific URL
    /// </summary>
    /// <param name="url"> url to download Achievement Image</param>
    /// <returns></returns>
    IEnumerator DownloadImage(string url)
    {
        UnityWebRequest requestTexture = UnityWebRequestTexture.GetTexture(url);
        yield return requestTexture.SendWebRequest();
        if (requestTexture.isNetworkError || requestTexture.isHttpError)
        {
            Debug.Log("[AchievementItem] DownloadImage(), Error Message: " + requestTexture.error);
        }
        else
        {
            SetIcon(((DownloadHandlerTexture)requestTexture.downloadHandler).texture);
        }
    }
    
    /// <summary>
    /// Set image after download finished
    /// </summary>
    /// <param name="textureDownloaded"> texture to set the image</param>
    private void SetIcon(Texture2D textureDownloaded)
    {
        icon.sprite = Sprite.Create(textureDownloaded, new Rect(0, 0, icon.preferredWidth, icon.preferredHeight), new Vector2(.5f, .5f));
        Color tempColor = icon.color;
        tempColor.a = 1;
        icon.color = tempColor;
    }
}
