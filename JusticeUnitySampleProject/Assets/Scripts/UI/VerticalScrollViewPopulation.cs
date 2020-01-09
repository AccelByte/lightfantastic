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
        
        var objectHeight = samplePrefab.GetComponent<RectTransform>().sizeDelta.y;
        for (int i = 0 ; i < requiredPrefab ; i++)
        {
            int index = i;
            GameObject prefab = Instantiate(samplePrefab);
            prefab.transform.SetParent(scrollViewContentContainer.transform, false);
            prefab.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            prefab.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            prefab.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, (-index - 0.5f) * objectHeight, 0);
        }

        return scrollViewContentContainer.GetComponentsInChildren<T>();
    }

    /// <summary>
    /// Check whether the scrollview is scrollable or not
    /// </summary>
    /// <param name="samplePrefab">Prefab game object</param>
    /// <param name="scrollViewContentContainer">Prefab containers</param>
    /// <returns></returns>
    public static bool IsVerticalScrollable(GameObject samplePrefab, RectTransform scrollViewContentContainer)
    {
        var objectHeight = samplePrefab.GetComponent<RectTransform>().sizeDelta.y;
        int prefabCount = scrollViewContentContainer.GetComponentsInChildren<T>().Length;
        var totalChildHeight = objectHeight * prefabCount;

        var containerHeight = scrollViewContentContainer.rect.height;

        return totalChildHeight > containerHeight;
    }
}
