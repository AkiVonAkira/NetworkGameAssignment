using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LobbyButton : MonoBehaviour
{
    public TMP_Text hostNameText;
    public Button joinButton;

    public void Initialize(FixedString64Bytes hostName, UnityAction joinAction)
    {
        hostNameText.text = hostName.ToString();
        joinButton.onClick.AddListener(joinAction);
    }
}