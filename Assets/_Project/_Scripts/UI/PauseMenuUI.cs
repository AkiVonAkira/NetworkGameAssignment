using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI
{
    public class PauseMenuUI : Singleton<PauseMenuUI>
    {
        [SerializeField] private Button quitButton;
        [SerializeField] private Button leaveServerButton;
        [SerializeField] private Canvas pauseMenuCanvas;
        [SerializeField] private NetworkManagerUI networkManagerUI;

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
            pauseMenuCanvas.enabled = !pauseMenuCanvas.enabled;
            Time.timeScale = pauseMenuCanvas.enabled ? 0 : 1;
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
    }
}
