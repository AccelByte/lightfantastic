using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUDManager<EnumT> : MonoBehaviour where EnumT : System.Enum
{
    protected readonly Dictionary<EnumT, BaseHUD> panels = new Dictionary<EnumT, BaseHUD>();

    [SerializeField]
    protected List<EnumT> types = null;
    [SerializeField]
    protected List<BaseHUD> panelObjects = null;
    [SerializeField]
    protected float distanceBetweenPanels = 0.1f;
    protected bool panelsPopulated_;
    List<BaseHUD> panelStack = null;
    protected Canvas mainCanvas_;
    protected virtual void Awake()
    {
        panelStack = new List<BaseHUD>();
        mainCanvas_ = GetComponent<Canvas>();
        PopulateHudDict();
    }

    protected virtual void Start()
    {
        Debug.Log("Base HUDManager start");
        HideAllPanels();
    }

    private void HideAllPanels()
    {
        if (!panelsPopulated_)
        {
            Debug.LogError("Misconfiguration in HUDManager, cannot continue");
            return;
        }
        foreach (var panel in panels)
        {
            Debug.Log("Attempt to hide panel: " + panel.Value.gameObject.name);
            if (panel.Value.gameObject.activeInHierarchy)
            {
                Debug.Log("Hiding panel: " + panel.Value.gameObject.name);
                panel.Value.gameObject.SetActive(false);
            }
        }
    }

    private void PopulateHudDict()
    {
        if (types.Count != panelObjects.Count)
        {
            Debug.LogError("Different Types and Panel Objects Count");
            return;
        }
        for (int i = 0; i < Mathf.Min(types.Count, panelObjects.Count); i++)
        {
            if (!panels.ContainsKey(types[i]))
            {
                panels.Add(types[i], panelObjects[i]);
            }
            else
            {
                Debug.LogError("Multiple Panel have the same type");
                return;
            }
        }
        panelsPopulated_ = true;
    }

    protected void ShowPanel(EnumT type, object[] args, bool resetStack = false, bool replace = false, bool blend = false)
    {
        if (!panelsPopulated_)
        {
            Debug.LogError("Misconfiguration in HUDManager, cannot continue");
            return;
        }
        BaseHUD tgtPanel = panels[type];
        tgtPanel.SetupData(args);
        Push(tgtPanel, resetStack, replace, blend);
    }

    protected void HideTopPanel()
    {
        if (!panelsPopulated_)
        {
            Debug.LogError("Misconfiguration in HUDManager, cannot continue");
            return;
        }
        Pop();
    }

    public T FindPanel<T>()
    {
        T panel = GetComponentInChildren<T>();
        return panel;
    }
    
    private void Push(BaseHUD tgt, bool resetStack, bool replace, bool blend)
    {
        if (!panelsPopulated_)
        {
            Debug.LogError("Misconfiguration in HUDManager, cannot continue");
            return;
        }
        if (resetStack)
        {
            foreach (var panel in panelStack)
            {
                if (panel.gameObject.activeInHierarchy)
                {
                    panel.gameObject.SetActive(false);
                }
            }
            panelStack.Clear();
        }
        int stackHeight = panelStack.Count;
        if (replace && stackHeight > 0)
        {
            BaseHUD currentTop = panelStack[stackHeight - 1];
            Vector3 currentTopPos = currentTop.rectTransform.localPosition;
            Vector3 tgtNewPos = tgt.rectTransform.localPosition;
            tgtNewPos.z = currentTopPos.z;
            tgt.rectTransform.localPosition = tgtNewPos;
            currentTop.ResetToOriginalPos();
            currentTop.gameObject.SetActive(false);
            tgt.gameObject.SetActive(true);
            panelStack[stackHeight - 1] = tgt;
        }
        else
        {
            Vector3 newPos = tgt.rectTransform.localPosition;
            newPos.z = (stackHeight > 0) ? (newPos.z + distanceBetweenPanels) : 0.0f;
            tgt.rectTransform.localPosition = newPos;
            if (!blend && stackHeight > 0)
            {
                panelStack[stackHeight - 1].gameObject.SetActive(false);
            }
            tgt.gameObject.SetActive(true);
            panelStack.Add(tgt);
        }
        Debug.Log("Stack count: " + panelStack.Count);
    }

    private void Pop()
    {
        if (!panelsPopulated_)
        {
            Debug.LogError("Misconfiguration in HUDManager, cannot continue");
            return;
        }
        int stackHeight = panelStack.Count;
        if (stackHeight == 0)
        {
            Debug.LogError("Empty HUD Stack, skipping operation");
            return;
        }
        BaseHUD currentTop = panelStack[stackHeight - 1];
        currentTop.gameObject.SetActive(false);
        currentTop.ResetToOriginalPos();
        if (stackHeight > 1)
        {
            BaseHUD newTop = panelStack[stackHeight - 2];
            if (!newTop.gameObject.activeInHierarchy)
            {
                newTop.gameObject.SetActive(true);
            }
        }
        panelStack.RemoveAt(stackHeight - 1);
        Debug.Log("Stack count: " + panelStack.Count);
    }
}
