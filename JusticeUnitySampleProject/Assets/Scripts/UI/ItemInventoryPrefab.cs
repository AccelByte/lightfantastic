#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;

public class ItemInventoryPrefab : MonoBehaviour
{
    [SerializeField]
    public Text title;
    [SerializeField]
    public Text count;
    [SerializeField]
    public Text description;
    [SerializeField]
    public Image image;
}
