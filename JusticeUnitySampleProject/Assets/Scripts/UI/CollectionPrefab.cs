using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    public void UpdateInfo(string category, int current, int total)
    {
        itemCategory = category;
        currentItemCount = current;
        totalItemCount = total;

        itemCategoryText.GetComponent<TextMeshProUGUI>().text = itemCategory;
        currentItemCountText.GetComponent<TextMeshProUGUI>().text = currentItemCount.ToString();
        totalItemCountText.GetComponent<TextMeshProUGUI>().text = totalItemCount.ToString();
    }
}
