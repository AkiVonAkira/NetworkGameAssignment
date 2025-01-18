using System;
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
        
        public string playerName;
        public bool isChatOpen;

        private void Start()
        {
            chatPanel.SetActive(false);
        }

        private void Update()
        {
            if (PauseMenuUI.Instance.IsPaused) return;
            
            if (Input.GetKeyDown(KeyCode.T) && IsOwner)
            {
                OpenChat();
            }
            
            if (Input.GetKeyDown(KeyCode.Return) && IsOwner)
            {
                SendChatMessage(chatInputField.text, playerName);
                CloseChat();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape) && IsOwner)
            {
                CloseChat();
            }
        }

        private void OpenChat()
        {
            isChatOpen = true;
            chatInputField.ActivateInputField();
            UnlockCursor();
        }

        private void CloseChat()
        {
            isChatOpen = false;
            chatInputField.text = string.Empty;
            chatInputField.DeactivateInputField();
            LockCursor();
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void SendChatMessage(string message, string fromWho = null)
        {
            if (string.IsNullOrEmpty(message)) return;
            
            Color nameColor = GenerateColorFromName(fromWho);
            string colorHex = ColorUtility.ToHtmlStringRGB(nameColor);
            string formattedFromWho = $"<color=#{colorHex}>{fromWho}</color>";
            string s = formattedFromWho + " > " + message;
            
            SendChatMessageServerRpc(s);
        }

        private void AddMessage(string msg)
        {
            ChatMessage cm = Instantiate(chatMessagePrefab, chatMessageParent);
            cm.SetText(msg);
        }

        private Color GenerateColorFromName(string name)
        {
            // Use a hash function to generate a consistent color for each name
            int hash = name.GetHashCode();
            float r = ((hash >> 16) & 0xFF) / 255.0f;
            float g = ((hash >> 8) & 0xFF) / 255.0f;
            float b = (hash & 0xFF) / 255.0f;
            return new Color(r, g, b);
        }
        
        [ServerRpc(RequireOwnership = false)]
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