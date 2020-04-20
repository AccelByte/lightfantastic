// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

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
