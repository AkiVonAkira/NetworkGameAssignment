using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace _Project
{
    public class CameraSwitcher : MonoBehaviour
    {
        [Header("Cinemachine Virtual Cameras")]
        public CinemachineCamera[] virtualCameras;

        [Header("Transition Settings")] 
        public float transitionDuration = 2f;
        public float waitTimeAtEachCamera = 3f;

        private CinemachineBrain _cinemachineBrain;
        private int _currentCameraIndex;

        private void Start()
        {
            if (Camera.main != null) _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
            if (_cinemachineBrain == null)
            {
                Debug.LogError("Main Camera is missing a CinemachineBrain component.");
                return;
            }

            // Activate the first camera and start the switching coroutine
            ActivateCamera(0);
            StartCoroutine(SwitchCamerasRoutine());
        }

        private IEnumerator SwitchCamerasRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(waitTimeAtEachCamera);

                _currentCameraIndex = (_currentCameraIndex + 1) % virtualCameras.Length;
                ActivateCamera(_currentCameraIndex);

                yield return new WaitForSeconds(transitionDuration);
            }
        }

        private void ActivateCamera(int index)
        {
            for (var i = 0; i < virtualCameras.Length; i++)
            {
                virtualCameras[i].Priority = i == index ? 5 : 0;
            }
        }
    }
}
