// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDMiniMap : MonoBehaviour
{
    [SerializeField]
    private GameObject minimapSliderPrefab;

    private List<GameObject> positionIndicators;

    private const uint SLIDER_HORIZONTAL_OFFSET = 40;

    private void Awake()
    {
        positionIndicators = new List<GameObject>();

        if (positionIndicators.Count <= 0)
        {
            Debug.Log("HUDMiniMap awake there is no positionIndicators available");
        }
    }

    public void SetupMinimap(string playerName, uint indexPlayer)
    {
        GameObject obj = Instantiate(minimapSliderPrefab, transform);
        
        if (obj != null)
        {
            if (positionIndicators.Count > 0)
            {
                // make sure there is no dupe
                for (int i = 0; i < positionIndicators.Count; i++)
                {
                    if (positionIndicators[i].GetComponent<MinimapSliderPrefab>().PlayerName.Contains(playerName))
                    {
                        Debug.Log("HUDMiniMap SetupMinimap there is already one player named:" + playerName);
                        return;
                    }
                }
            }

            // arage top or bottom side
            if ((positionIndicators.Count + 1) % 2 == 0)
            {
                obj.GetComponent<MinimapSliderPrefab>().SetupSliderUI(playerName, indexPlayer);
            }
            else
            {
                obj.GetComponent<MinimapSliderPrefab>().SetupSliderUI(playerName, indexPlayer, E_MinimapSliderPosition.MiniMapSliderTop);
            }

            obj.GetComponent<Slider>().minValue = SLIDER_HORIZONTAL_OFFSET;
            obj.GetComponent<Slider>().maxValue = LightFantasticConfig.FINISH_LINE_DISTANCE + SLIDER_HORIZONTAL_OFFSET;
            positionIndicators.Add(obj);
            // update if Index position is 0, set background panel of sliders to off
            if (positionIndicators.Count - 1 == 0)
            {
                positionIndicators[0].transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                positionIndicators[positionIndicators.Count - 1].transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    public void UpdatePlayerPositionIndicator(string playerName, uint position)
    {
        if (positionIndicators.Count > 0)
        {
            for (int i = 0; i < positionIndicators.Count; i++)
            {
                if (positionIndicators[i].GetComponent<MinimapSliderPrefab>().PlayerName.Contains(playerName))
                {
                    // update position
                    positionIndicators[i].GetComponent<Slider>().value = position;
                    // update index to the current rank index by sibling index
                    for (int j = 0; j < positionIndicators.Count; j++)
                    {
                        if (j != i) { 
                            if (positionIndicators[i].GetComponent<Slider>().value > positionIndicators[j].GetComponent<Slider>().value)
                            {
                                if (positionIndicators[i].transform.GetSiblingIndex() > positionIndicators[j].transform.GetSiblingIndex())
                                {
                                    positionIndicators[i].transform.SetSiblingIndex(positionIndicators[j].transform.GetSiblingIndex());
                                }
                            }
                        }
                    }
                    // Set Background Panel for sliders to off if index not 0
                    if (positionIndicators[i].transform.GetSiblingIndex() == 0)
                    {
                        positionIndicators[i].transform.GetChild(0).gameObject.SetActive(true);
                        for (int c = 0; c < positionIndicators.Count; c++)
                        {
                            if (c != i)
                            {
                                positionIndicators[c].transform.GetChild(0).gameObject.SetActive(false);
                            }
                        }
                    }
                    break;
                }
            }
        }
        else
        {
            Debug.Log("HUDMinimap UpdatePlayersPositionIndicator positionIndicators is empty");
        }
    }

    public void RemovePlayerPositionIndicator(string playerName)
    {
        if (positionIndicators.Count > 0)
        {
            for (int i = 0; i < positionIndicators.Count; i++)
            {
                if (positionIndicators[i].GetComponent<MinimapSliderPrefab>().PlayerName.Contains(playerName))
                {
                    // Remove from list and destroy the object
                    GameObject obj =  positionIndicators[i].gameObject;
                    positionIndicators.RemoveAt(i);
                    Destroy(obj);
                }
            }
        }
        else
        {
            Debug.Log("HUDMinimap UpdatePlayersPositionIndicator positionIndicators is empty");
        }
    }
}
