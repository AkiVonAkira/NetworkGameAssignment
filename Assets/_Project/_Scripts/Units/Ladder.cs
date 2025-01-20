using UnityEngine;

namespace _Project
{
    public class Ladder : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var controller = other.GetComponent<FirstPersonController>();
                if (controller != null)
                {
                    controller.SetClimbing(true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var controller = other.GetComponent<FirstPersonController>();
                if (controller != null)
                {
                    controller.SetClimbing(false);
                }
            }
        }
    }
}