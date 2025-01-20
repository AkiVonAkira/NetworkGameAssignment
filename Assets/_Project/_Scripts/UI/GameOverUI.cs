using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project
{
    public class GameOverUI : Singleton<GameOverUI>
    {
        [SerializeField] internal TextMeshProUGUI winnerText;
        [SerializeField] internal Button rematchButton;
        [SerializeField] private Button leaveButton;
        
        private Canvas _canvas;
        internal bool IsMenuOpen;

        private new void Awake()
        {
            base.Awake();
            rematchButton.onClick.AddListener(OnRematchButtonClicked);
            leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        }

        private void Start()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false;
        }

        private void OnRematchButtonClicked()
        {
            GameManager.Instance.Rematch();
        }
        
        private void OnLeaveButtonClicked()
        {
            PauseMenuUI.Instance.LeaveServer();
        }

        public void Show()
        {
            _canvas.enabled = true;
            IsMenuOpen = true;
        }
        
        public void Hide()
        {
            _canvas.enabled = false;
            IsMenuOpen = false;
        }
    }
}