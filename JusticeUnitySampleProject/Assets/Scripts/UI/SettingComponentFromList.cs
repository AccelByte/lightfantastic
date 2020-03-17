using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class SettingComponentFromList : MonoBehaviour
{
    private uint currentIndex;
    
    private string[] dictionary;
    
    /// <summary>
    /// Callback return index of dictionary
    /// </summary>
    private UnityAction<uint> onStateSelected;

    [SerializeField] private Text stateLabel;
    [Header("Right side component")]
    [SerializeField] private Button rightButton;
    [SerializeField] private Image rightTriangleImage;
    [Header("Left side component")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Image leftTriangleImage;
    
    public void Init(string[] _dictionary, UnityAction<uint> _onStateSelected, uint initialSelectedIndex)
    {
        dictionary = _dictionary;
        onStateSelected = _onStateSelected;
        
        rightButton.onClick.RemoveAllListeners();
        leftButton.onClick.RemoveAllListeners();
        
        rightButton.onClick.AddListener(OnRightButtonClicked);
        leftButton.onClick.AddListener(OnLeftButtonClicked);
        SelectSettingComponent(initialSelectedIndex);
    }

    private void SelectSettingComponent(uint index)
    {
        for (uint i = 0; i < dictionary.Length; i++)
        {
            if (i == index)
            {
                stateLabel.text = dictionary[i];
                currentIndex = i;
                onStateSelected(i);
            }
        }
    }

    private void OnLeftButtonClicked()
    {
        currentIndex -= 1;
        SelectSettingComponent(currentIndex);
        CheckIndex(currentIndex);
    }

    private void OnRightButtonClicked()
    {
        currentIndex += 1;
        SelectSettingComponent(currentIndex);
        CheckIndex(currentIndex);
    }

    private void CheckIndex(uint i)
    {
        if (i == 0)
        {
            SetEnableLeftButton(false);
        }
        else if (i == dictionary.Length - 1)
        {
            SetEnableRightButton(false);
        }
        else
        {
            SetEnableLeftButton(true);
            SetEnableRightButton(true);
        }
    }

    private void SetEnableLeftButton(bool enable)
    {
        leftButton.interactable = enable;
        leftTriangleImage.color = enable ? Color.white : new Color(1,1,1,0.3f);
    }

    private void SetEnableRightButton(bool enable)
    {
        rightButton.interactable = enable;
        rightTriangleImage.color = enable ? Color.white : new Color(1,1,1,0.3f);
    }
}
