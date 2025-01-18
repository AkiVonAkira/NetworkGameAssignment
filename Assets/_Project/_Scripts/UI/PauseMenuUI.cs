using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace _Project
{
    public class PauseMenuUI : Singleton<PauseMenuUI>
    {
        [SerializeField] private Button quitButton;
        [SerializeField] private Button leaveServerButton;
        [SerializeField] internal Canvas pauseMenuCanvas;
        [SerializeField] private NetworkManagerUI networkManagerUI;
        [SerializeField] private ChatManager chatManager;
        
        internal bool IsPaused => pauseMenuCanvas.enabled;

        private new void Awake()
        {
            base.Awake();
            quitButton.onClick.AddListener(QuitGame);
            leaveServerButton.onClick.AddListener(LeaveServer);
        }

        private void Start()
        {
            pauseMenuCanvas.enabled = false;
        }

        public void TogglePauseMenu()
        {
            if (chatManager.isChatOpen) return;

            pauseMenuCanvas.enabled = !pauseMenuCanvas.enabled;
            Time.timeScale = pauseMenuCanvas.enabled ? 0 : 1;

            if (pauseMenuCanvas.enabled)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }

        private void QuitGame()
        {
            Application.Quit();
        }

        private void LeaveServer()
        {
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
                NetworkManager.Singleton.Shutdown();

            networkManagerUI.networkManagerCanvas.enabled = true;
            pauseMenuCanvas.enabled = false;
            Time.timeScale = 1;
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
    }
}
