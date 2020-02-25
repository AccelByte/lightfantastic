using TMPro;
using UnityEngine;

public class MainHUDTimerPrefab : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI number;
    [SerializeField] private TextMeshProUGUI sec;

    private void Start()
    {
        number.text = "";
        sec.text = "";
    }

    public void SetTime(int time)
    {
        number.text = $"{time}";
        sec.text = "sec";
    }
}
