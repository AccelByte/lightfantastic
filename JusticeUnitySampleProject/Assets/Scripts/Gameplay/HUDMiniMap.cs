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

    public void SetupMinimap(string playerName)
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
                obj.GetComponent<MinimapSliderPrefab>().SetupSliderUI(playerName);
            }
            else
            {
                obj.GetComponent<MinimapSliderPrefab>().SetupSliderUI(playerName,E_MinimapSliderPosition.MiniMapSliderTop);
            }

            obj.GetComponent<Slider>().minValue = SLIDER_HORIZONTAL_OFFSET;
            obj.GetComponent<Slider>().maxValue = LightFantasticConfig.FINISH_LINE_DISTANCE + SLIDER_HORIZONTAL_OFFSET;
            positionIndicators.Add(obj);
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
