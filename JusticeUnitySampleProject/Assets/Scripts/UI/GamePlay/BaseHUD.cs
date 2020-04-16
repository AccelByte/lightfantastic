// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;

public abstract class BaseHUD : MonoBehaviour, IHUD
{
    protected Vector3 originalPos;
    protected RectTransform rectTr_ = null;
    protected Canvas canvas_;
    public abstract void OnShow();
    public abstract void OnHide();
    protected abstract void AddListeners();
    protected virtual void Awake()
    {
        rectTr_ = GetComponent<RectTransform>();
        canvas_ = GetComponent<Canvas>();
        SaveOriginalPosition();
        Debug.Log("Base.Awake");
    }

    protected virtual void Start()
    {
        AddListeners();
    }
    
    private void OnEnable()
    {
        OnShow();
    }

    private void OnDisable()
    {
        OnHide();
    }

    protected void SaveOriginalPosition()
    {
        originalPos = rectTr_.localPosition;
    }

    public void ResetToOriginalPos()
    {
        rectTr_.localPosition = originalPos;
    }

    public abstract void SetupData(object[] args);
    public RectTransform rectTransform { get { return rectTr_; } }
    public bool IsShowing { get { return gameObject.activeInHierarchy; } }
}
