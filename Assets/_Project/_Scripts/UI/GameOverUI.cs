using UnityEngine;
using UnityEngine.UI;

namespace _Project
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private Text winnerText;
        [SerializeField] private Button rematchButton;
        [SerializeField] private Button leaveButton;

        private void Awake()
        {
            rematchButton.onClick.AddListener(OnRematchButtonClicked);
            leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        }

        private void OnRematchButtonClicked()
        {
            GameManager.Instance.Rematch();
        }
        
        private void OnLeaveButtonClicked()
        {
            PauseMenuUI.Instance.LeaveServer();
        }
    }
}