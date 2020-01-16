using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionPrefab : MonoBehaviour
{
    private Transform currentItemCountText;
    private Transform totalItemCountText;
    private Transform itemCategoryText;
    [SerializeField]
    private int currentItemCount = 1;
    [SerializeField]
    private int totalItemCount = 3;
    [SerializeField]
    private string itemCategory = "Head Gear";

    void Awake()
    {
        itemCategoryText = transform.Find("ItemCategory");
        currentItemCountText = transform.Find("CurrentItems");
        totalItemCountText = transform.Find("TotalItems");
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateInfo(itemCategory, currentItemCount, totalItemCount);
    }

    // Update Info
    void UpdateInfo(string category, int current, int total)
    {
        itemCategory = category;
        currentItemCount = current;
        totalItemCount = total;

        itemCategoryText.GetComponent<TMPro.TextMeshProUGUI>().text = itemCategory;
        currentItemCountText.GetComponent<TMPro.TextMeshProUGUI>().text = currentItemCount.ToString();
        totalItemCountText.GetComponent<TMPro.TextMeshProUGUI>().text = totalItemCount.ToString();
    }
}
