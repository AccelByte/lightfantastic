// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

#pragma warning disable 0649

using System;
using System.Linq;
using UnityEngine;

public class VerticalScrollViewPopulation<T> : MonoBehaviour
{
    /// <summary>
    /// Create N amount of prefab in the targeted rect transform
    /// </summary>
    /// <param name="count">Population amount</param>
    /// <param name="samplePrefab">Prefab game object</param>
    /// <param name="scrollViewContentContainer">Content of scroll view</param>
    public static T[] Populate(int count, GameObject samplePrefab, RectTransform scrollViewContentContainer)
    {
        int existingPrefab = scrollViewContentContainer.gameObject.transform.childCount;
        int requiredPrefab = 0;
                
        if (existingPrefab <= count)
        {
            requiredPrefab = count - existingPrefab;
        }
        else //need to delete exisiting item
        {
            while(existingPrefab > count)
            {
                DestroyImmediate(scrollViewContentContainer.gameObject.transform.GetChild(0).gameObject);
            }
        }
        
        // Scale
        var childWidth = samplePrefab.GetComponent<RectTransform>().sizeDelta.y;
        var containerWidth = scrollViewContentContainer.GetComponent<RectTransform>().sizeDelta.x;
        float scale = Math.Abs(containerWidth / childWidth);
        
        for (int i = 0 ; i < requiredPrefab ; i++)
        {
            int index = i;
            GameObject prefab = Instantiate(samplePrefab);
            prefab.gameObject.GetComponent<RectTransform>().localScale = new Vector3(scale, scale, scale);
            prefab.transform.SetParent(scrollViewContentContainer.transform, false);
        }

        return scrollViewContentContainer.GetComponentsInChildren<T>();
    }

    /// <summary>
    /// Check whether the scrollview is scrollable or not
    /// </summary>
    /// <param name="samplePrefab">Prefab game object</param>
    /// <param name="scrollViewContentContainer">Prefab containers</param>
    /// <returns></returns>
    public static bool IsVerticalScrollable(RectTransform scrollViewContentContainer)
    {
        var childTransforms = scrollViewContentContainer.GetComponentsInChildren<T>().Cast<MonoBehaviour>().ToList();
        var totalChildHeight = 0.0f;
        foreach (var child in childTransforms)
        {
            var childDeltaY = child.GetComponent<RectTransform>().sizeDelta.y;
            var childScaleY = child.GetComponent<RectTransform>().localScale.y;
            totalChildHeight += childDeltaY * childScaleY;
        }

        var containerHeight = scrollViewContentContainer.rect.height;

        return totalChildHeight > containerHeight;
    }
}
