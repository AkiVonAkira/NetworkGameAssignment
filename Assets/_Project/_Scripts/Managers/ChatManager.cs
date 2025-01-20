using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace _Project
{
    public class ChatManager : NetworkSingleton<ChatManager>
    {
        [SerializeField] internal GameObject chatPanel;
        [SerializeField] private ChatMessage chatMessagePrefab;
        [SerializeField] private Transform chatMessageParent;
        [SerializeField] private TMP_InputField chatInputField;
        private InputSystem _inputSystem;

        public string playerName;
        public bool isChatOpen;

        private void Start()
        {
            chatPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T) && IsOwner) OpenChat();

            if (Input.GetKeyDown(KeyCode.Return) && IsOwner)
            {
                SendChatMessage(chatInputField.text, playerName);
                CloseChat();
            }

            if (Input.GetKeyDown(KeyCode.Escape) && IsOwner) CloseChat();
        }
        
        public void SetInputSystem(InputSystem inputSystem)
        {
            _inputSystem = inputSystem;
        }
        
        private void OpenChat()
        {
            if (PauseMenuUI.Instance.IsPaused) return;
            isChatOpen = true;
            chatPanel.SetActive(true);
            chatInputField.gameObject.SetActive(true);
            chatInputField.ActivateInputField();
            chatInputField.onFocusSelectAll = true;
            chatInputField.Select();
            UnlockCursor();
            _inputSystem.enabled = false;
        }

        private void CloseChat()
        {
            if (PauseMenuUI.Instance.IsPaused) return;
            isChatOpen = false;
            chatInputField.text = string.Empty;
            chatInputField.DeactivateInputField();
            LockCursor();
            _inputSystem.enabled = true;
        }

        private void LockCursor()
        {
            _inputSystem.SetCursorState(false);
        }

        private void UnlockCursor()
        {
            _inputSystem.SetCursorState(false);
        }

        public void SendChatMessage(string message, string fromWho = null)
        {
            if (string.IsNullOrEmpty(message)) return;

            var nameColor = GenerateColorFromName(fromWho);
            var colorHex = ColorUtility.ToHtmlStringRGB(nameColor);
            var formattedFromWho = $"<color=#{colorHex}>{fromWho}</color>";
            var s = formattedFromWho + " > " + message;

            SendChatMessageServerRpc(s);
        }

        private void AddMessage(string msg)
        {
            var cm = Instantiate(chatMessagePrefab, chatMessageParent);
            cm.SetText(msg);
        }

        private Color GenerateColorFromName(string name)
        {
            // Use a hash function to generate a consistent color for each name
            var hash = name.GetHashCode();
            var r = ((hash >> 16) & 0xFF) / 255.0f;
            var g = ((hash >> 8) & 0xFF) / 255.0f;
            var b = (hash & 0xFF) / 255.0f;
            return new Color(r, g, b);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SendChatMessageServerRpc(string message)
        {
            ReceiveChatMessageClientRpc(message);
        }

        [ClientRpc]
        private void ReceiveChatMessageClientRpc(string message)
        {
            Instance.AddMessage(message);
        }
    }
}