// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileAttributePrefab : MonoBehaviour
{
    [SerializeField]
    private Transform AttributeName;
    [SerializeField]
    private Transform AttributeValue;



    void Awake()
    {
        AttributeName = transform.Find("AttributeNameText");
        AttributeValue = transform.Find("AttributeValueText ");
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetupProfileAttributeUI(string attributeName, string attributeValue)
    {
        this.AttributeName.GetComponent<Text>().text = attributeName;
        this.AttributeValue.GetComponent<Text>().text = attributeValue;
    }

    public void OnClearProfileButton()
    {
        // TODO do on destroy functions
    }
}
