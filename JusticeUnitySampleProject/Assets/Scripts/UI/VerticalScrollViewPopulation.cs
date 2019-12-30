using UnityEngine;

public class VerticalScrollViewPopulation<T> : MonoBehaviour
{
    /// <summary>
    /// Create N amount of prefab in the targeted rect transform
    /// </summary>
    /// <param name="count">Population amount</param>
    /// <param name="samplePrefab">Prefab game object </param>
    /// <param name="scrollViewContentContainer">Content of scroll view</param>
    public static void Populate(int count, GameObject samplePrefab, RectTransform scrollViewContentContainer)
    {
        var objectHeight = samplePrefab.GetComponent<RectTransform>().sizeDelta.y;
        for (int i = 0 ; i < count ; i++)
        {
            int index = i;
            GameObject prefab = Instantiate(samplePrefab);
            prefab.transform.SetParent(scrollViewContentContainer.transform, false);
            prefab.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            prefab.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            prefab.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, (-index - 0.5f) * objectHeight, 0);
        }
    }
}
