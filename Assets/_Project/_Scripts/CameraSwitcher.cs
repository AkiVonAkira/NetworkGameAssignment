using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cinemachine Virtual Cameras")]
    public CinemachineCamera[] virtualCameras; // Add your vCams here in the inspector

    [Header("Transition Settings")] public float transitionDuration = 2f; // Time for blending between cameras

    public float waitTimeAtEachCamera = 3f; // Time to stay at each camera

    private CinemachineBrain _cinemachineBrain;
    private int _currentCameraIndex;

    private void Start()
    {
        // Ensure there is a CinemachineBrain in the scene
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

    private void ActivateCamera(int index)
    {
        // Deactivate all cameras except the one at the specified index
        for (var i = 0; i < virtualCameras.Length; i++) virtualCameras[i].Priority = i == index ? 10 : 0;
    }

    private IEnumerator SwitchCamerasRoutine()
    {
        while (true)
        {
            // Wait for the specified time at the current camera
            yield return new WaitForSeconds(waitTimeAtEachCamera);

            // Move to the next camera in the array
            _currentCameraIndex = (_currentCameraIndex + 1) % virtualCameras.Length;

            // Activate the next camera
            ActivateCamera(_currentCameraIndex);

            // Wait for the transition to complete
            yield return new WaitForSeconds(transitionDuration);
        }
    }
}
