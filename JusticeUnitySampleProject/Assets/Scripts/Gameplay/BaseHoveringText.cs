using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseHoveringText : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The prefab of hovertext.")]
    private GameObject hoveringText = null;
    [SerializeField]
    [Tooltip("The text to display.")]
    private string textInput = "";
    [SerializeField]
    private Vector3 offsetPosition = Vector3.up;
    [SerializeField]
    private bool isUsingMainCamera = true;
    [SerializeField]
    private Camera alternativeCamera = null;

    private Camera currentCamera;
    private GameObject currentText;

    // Start is called before the first frame update
    void Start()
    {
        if (isUsingMainCamera)
        {
            currentCamera = Camera.main;
        }
        else
        {
            currentCamera = alternativeCamera;
        }
 
        currentText = Instantiate(hoveringText, transform);        
        currentText.transform.SetParent(GameObject.Find("HoverTextPanel").transform, false);
        currentText.GetComponent<Text>().text = textInput;
    }

    void OnDestroy()
    {
        if (currentText != null)
        {
            Destroy(currentText);
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentText.transform.position = currentCamera.WorldToScreenPoint(transform.position + offsetPosition);
    }

    public void ChangeTextLabel(string text)
    {
        currentText.GetComponent<Text>().text = text;
    }
}
