// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using UnityEngine.UI;

public class PromptPanelComponent : MonoBehaviour
{
    [SerializeField] public AccelByteButtonScriptStyle primaryButton;
    [SerializeField] public AccelByteButtonScriptStyle secondaryButton;
    [SerializeField] public GameObject closeButtonGameObject;
    [SerializeField] public AccelByteButtonScriptStyle closeButton;
    [SerializeField] public Text headerText;
    [SerializeField] public Text descriptionText;
    [SerializeField] public CanvasGroup promptPanelCanvasGroup;
}
