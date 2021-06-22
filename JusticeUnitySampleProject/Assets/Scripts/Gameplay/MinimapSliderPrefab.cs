// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public enum E_MinimapSliderPosition
{
    MiniMapSliderTop,
    MiniMapSliderBottom
}

public class MinimapSliderPrefab : MonoBehaviour
{
    [HideInInspector]
    public string PlayerName;
    public TextMeshProUGUI PlayerNameText;
    [SerializeField]
    private GameObject Handle;

    private const float NAMETAG_VERTICAL_OFFSET = 25.0f;

    public void SetupSliderUI(string playerName, uint indexPlayer, E_MinimapSliderPosition e_MinimapSlider = E_MinimapSliderPosition.MiniMapSliderBottom)
    {
        PlayerName = playerName;
        PlayerNameText.text = playerName;

        switch (indexPlayer)
        {
            case 0:
                PlayerNameText.color = Color.red;
                Handle.transform.GetComponent<Image>().color = Color.red;
                break;
            case 1:
                PlayerNameText.color = Color.green;
                Handle.transform.GetComponent<Image>().color = Color.green;
                break;
            case 2:
                PlayerNameText.color = Color.yellow;
                Handle.transform.GetComponent<Image>().color = Color.yellow;
                break;
            case 3:
                PlayerNameText.color = Color.blue;
                Handle.transform.GetComponent<Image>().color = Color.blue;
                break;
        }

        switch (e_MinimapSlider)
        {
            case E_MinimapSliderPosition.MiniMapSliderTop:
                PlayerNameText.transform.localPosition = new Vector3(PlayerNameText.transform.localPosition.x, NAMETAG_VERTICAL_OFFSET, PlayerNameText.transform.localPosition.z);
                break;
            case E_MinimapSliderPosition.MiniMapSliderBottom:
                PlayerNameText.transform.localPosition = new Vector3(PlayerNameText.transform.localPosition.x, -(NAMETAG_VERTICAL_OFFSET), PlayerNameText.transform.localPosition.z);
                break;
            default:
                PlayerNameText.transform.localPosition = new Vector3(PlayerNameText.transform.localPosition.x, NAMETAG_VERTICAL_OFFSET, PlayerNameText.transform.localPosition.z);
                break;
        }
    }
}
