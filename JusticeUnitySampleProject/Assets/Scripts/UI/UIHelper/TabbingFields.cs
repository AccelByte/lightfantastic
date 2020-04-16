// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabbingFields : MonoBehaviour
{
    public List<InputField> inputFields = new List<InputField>();
    public int currentInput = 0;
    public Button continueButton;

    private void OnBecameInvisible()
    {
        enabled = false;
    }
    private void OnEnable()
    {
        currentInput = 0;
        for (int i = 0; i < inputFields.Count; i++)
        {
            inputFields[i].GetComponent<TweenComponent>().AnimateScaleToNormal();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            currentInput++;
            if(currentInput >= inputFields.Count)
            {
                currentInput = 0;
            }
            inputFields[currentInput].ActivateInputField();
        }

        if(Input.GetKeyDown(KeyCode.Return))
        {
            //progress
            continueButton.onClick.Invoke();
        }
    }

    public void SetActiveInput(int activeInput)
    {
        currentInput = activeInput;
    }
}
