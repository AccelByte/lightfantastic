using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    public void SetupSliderUI(string playerName, E_MinimapSliderPosition e_MinimapSlider = E_MinimapSliderPosition.MiniMapSliderBottom)
    {
        PlayerName = playerName;
        PlayerNameText.text = playerName;

        switch (e_MinimapSlider)
        {
            case E_MinimapSliderPosition.MiniMapSliderTop:
                PlayerNameText.transform.localPosition = new Vector3(PlayerNameText.transform.localPosition.x,25, PlayerNameText.transform.localPosition.z);
                break;
            case E_MinimapSliderPosition.MiniMapSliderBottom:
                PlayerNameText.transform.localPosition = new Vector3(PlayerNameText.transform.localPosition.x, -25, PlayerNameText.transform.localPosition.z);
                break;
            default:
                PlayerNameText.transform.localPosition = new Vector3(PlayerNameText.transform.localPosition.x, 25, PlayerNameText.transform.localPosition.z);
                break;
        }
    }
}
