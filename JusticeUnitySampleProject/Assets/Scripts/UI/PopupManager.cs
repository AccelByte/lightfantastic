using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PopupManager : MonoBehaviour
{
    private static PopupManager instance;
    public static PopupManager Instance { get { return instance; } }

    [Header("Popup Template")]
    [SerializeField]
    private GameObject prefabPopup;

    [SerializeField]
    private PopupData popupData;

    [SerializeField]
    private Transform popupRoot;

    private GameObject currentPopup;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);

        //popupRoot = this.transform;
    }

    private void InitPopup()
    {
        PopupPrefab popup = null;

        if (currentPopup == null)
        {
            currentPopup = Instantiate(prefabPopup, popupRoot);
        }

        popup = currentPopup.GetComponent<PopupPrefab>();
        if (popup != null)
        {
            popup.header = popupData.TittleText;
            popup.description = popupData.DescriptionText;
            popup.primaryButtonText = popupData.PrimaryButtonText;
            popup.primaryButtonAction = popupData.primaryButtonAction;
            popup.secondaryButtonText = popupData.SecondaryButtonText;
            popup.secondaryButtonAction = popupData.secondaryButtonAction;
            popup.showExitButton = true;
        }
    }

    public void InitPopup(string header, string desc, string btnText01, string btnText02, UnityAction btnCallback01, UnityAction btnCallback02)
    {
        Debug.Log("Popupmanager InitPopup Header: " + header);
        PopupPrefab popup = null;

        if (currentPopup == null)
        {
            currentPopup = Instantiate(prefabPopup, popupRoot);
        }
        
        popup = currentPopup.GetComponent<PopupPrefab>();
        if (popup != null)
        {
            popup.header = header;
            popup.description = desc;
            popup.primaryButtonText = btnText01;
            popup.primaryButtonAction = btnCallback01;
            popup.secondaryButtonText = btnText02;
            popup.secondaryButtonAction = btnCallback02;
            popup.showExitButton = true;
        }
    }

    public void ShowPopup()
    {
        Debug.Log("Popupmanager ShowPopup");
        if (currentPopup != null)
        {
            currentPopup.GetComponent<PopupPrefab>().Show();        
        }
    }

    public void HidePopup()
    {
        Debug.Log("Popupmanager HidePopup");
        if (currentPopup != null)
        {
            currentPopup.GetComponent<PopupPrefab>().Hide();
            ClearPopup();
        }
    }

    private void ClearPopup()
    {
        Debug.Log("Popupmanager ClearPopup");
        if (currentPopup != null)
        {
            PopupPrefab popup = currentPopup.GetComponent<PopupPrefab>();
            popup.header = "";
            popup.description = "";
            popup.primaryButtonText = "";
            popup.primaryButtonAction = null;
            popup.secondaryButtonText = "";
            popup.secondaryButtonAction = null;
            popup.showExitButton = false;
        }
    }
}
