// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;

public interface IHUD
{
    void OnShow();
    void OnHide();
    void SetupData(object[] args);
    RectTransform rectTransform { get; }
    bool IsShowing { get; }
}
