using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PopupData
{
    public E_PopupType PopupType;

    public string TittleText;

    public string DescriptionText;

    [Header("Primary Button")]
    public string PrimaryButtonText;
    public UnityAction primaryButtonAction;

    [Header("Secondary Button")]
    public string SecondaryButtonText;
    public UnityAction secondaryButtonAction;

    //[HideInInspector]
    //public AudioSource Source;
}
