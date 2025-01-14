using UnityEngine;

namespace _Project
{
    public class Ladder : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) other.GetComponent<FPSController>().IsClimbing = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) other.GetComponent<FPSController>().IsClimbing = false;
        }
    }
}
