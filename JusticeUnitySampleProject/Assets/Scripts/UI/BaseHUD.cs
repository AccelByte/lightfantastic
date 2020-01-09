using UnityEngine;

public abstract class BaseHUD : MonoBehaviour, IHUD
{
    public abstract void OnShow();
    public abstract void OnHide();
    protected abstract void AddListeners();
}
