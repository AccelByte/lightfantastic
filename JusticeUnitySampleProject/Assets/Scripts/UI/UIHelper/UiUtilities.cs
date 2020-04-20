// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UiUtilities : MonoBehaviour
{
    public void DownloadImage(string url, Image image)
    {
        StartCoroutine(DownloadImageEnumerator(url, image));
    }

    private static IEnumerator DownloadImageEnumerator(string url, Image image)
    {
        WWW www = new WWW(url);
        yield return www;
        Texture2D t2D = new Texture2D(www.texture.width, www.texture.height, TextureFormat.DXT5, false);
        www.LoadImageIntoTexture(t2D);
        image.sprite = Sprite.Create(t2D, new Rect(0,0 , t2D.width, t2D.height), new Vector2(0, 0));
        www.Dispose();
        www = null;
    }

    public void GetText(string url, Action<string> textCallback)
    {
        
        StartCoroutine(GetTextEnumerator(url, textCallback));
    }

    private static IEnumerator GetTextEnumerator(string url, Action<string> textCallback)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        if (!webRequest.isNetworkError && !webRequest.isHttpError)
        {
            textCallback(webRequest.downloadHandler.text);
        }
    }
}
