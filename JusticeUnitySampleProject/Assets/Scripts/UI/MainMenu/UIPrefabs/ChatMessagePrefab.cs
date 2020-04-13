#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;

public class ChatMessagePrefab : MonoBehaviour
{
    [SerializeField]
    private Text messageText;

    private string myColor;
    private string otherColor;

    private void Awake()
    {
        myColor = "<color=#818181>";
        otherColor = "<color=#FF39BD>";
    }

    public void WriteMessage(string playerName, string message, bool isMe)
    {
        if (isMe)
        {
            playerName = myColor + playerName + "</color>";
        }
        else
        {
            playerName = otherColor + playerName + "</color>";
        }
        messageText.text = playerName + ": " + message;
    }

    public void WriteWarning(string message)
    {
        messageText.text = otherColor + message + "</color>";
    }
}
