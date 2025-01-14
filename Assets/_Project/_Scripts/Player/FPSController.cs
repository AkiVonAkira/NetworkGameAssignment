using Unity.Netcode;
using UnityEngine;

namespace _Project
{
    [RequireComponent(typeof(CharacterController))]
    public class FPSController : NetworkBehaviour
    {
        public float walkingSpeed = 2.0f;
        public float runSpeed = 2.5f;
        public float crouchSpeed = 1.0f;
        public float lookSpeed = 2.0f;
        public float lookXLimit = 45.0f;
        public float climbSpeed = 3.0f;

        [Space(10)] public GameObject playerCamera;

        private CharacterController _characterController;
        private bool _isCrouching;
        private float _rotationX;
        internal bool IsClimbing;

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (IsOwner) HandleInput();
        }

        private void HandleInput()
        {
            HandleMovement();
            HandleCrouching();
            HandleClimbing();
        }

        private void HandleMovement()
        {
            var speed = _isCrouching ? crouchSpeed : walkingSpeed;
            var runSpeedCalculated = Input.GetKey(KeyCode.LeftShift) ? runSpeed : 1.0f;

            var forward = transform.TransformDirection(Vector3.forward);
            var right = transform.TransformDirection(Vector3.right);
            var moveDirection =
                forward * (speed * runSpeedCalculated * Input.GetAxis("Vertical")) +
                right * (speed * runSpeedCalculated * Input.GetAxis("Horizontal"));

            if (!_characterController.isGrounded && !IsClimbing) moveDirection += Physics.gravity;

            _characterController.Move(moveDirection * Time.deltaTime);

            _rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            _rotationX = Mathf.Clamp(_rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        private void HandleCrouching()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                _isCrouching = !_isCrouching;
                _characterController.height = _isCrouching ? 1.0f : 2.0f;
            }
        }

        private void HandleClimbing()
        {
            if (IsClimbing)
            {
                var vertical = Input.GetAxis("Vertical");
                var climbDirection = new Vector3(0, vertical * climbSpeed, 0);
                _characterController.Move(climbDirection * Time.deltaTime);

                if (_characterController.isGrounded && vertical > 0)
                {
                    IsClimbing = false;
                    _characterController.slopeLimit = 45.0f;
                }
            }
        }
    }
}
