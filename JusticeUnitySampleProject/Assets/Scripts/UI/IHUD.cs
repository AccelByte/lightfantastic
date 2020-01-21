using UnityEngine;

public interface IHUD
{
    void OnShow();
    void OnHide();
    void SetupData(object[] args);
    RectTransform rectTransform { get; }
    bool IsShowing { get; }
}
