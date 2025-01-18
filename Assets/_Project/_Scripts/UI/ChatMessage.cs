using TMPro;
using UnityEngine;

public class ChatMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;

    public void SetText(string str)
    {
        messageText.text = str;
    }
}