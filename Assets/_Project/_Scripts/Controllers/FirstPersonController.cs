using _Project.UI;
using Unity.Netcode;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace _Project
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : NetworkBehaviour
    {
        private const float Threshold = 0.01f;

        [Header("Player")]
        public float moveSpeed = 2.0f;
        public float sprintSpeed = 3.335f;
        public float crouchSpeed = 1.25f;
        public float climbSpeed = 8.0f;
        public float rotationSmoothTime = 0.12f;
        public float speedChangeRate = 10.0f;
        public float jumpHeight = 1.2f;
        public float gravity = -15.0f;
        public float jumpTimeout = 0.3f;
        public float fallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool grounded = true;
        public float groundedOffset = -0.14f;
        public float groundedRadius = 0.28f;
        public LayerMask groundLayers;

        [Header("Camera")]
        public GameObject playerCamera;
        public float topClamp = 70.0f;
        public float bottomClamp = -30.0f;
        public float cameraAngleOverride;
        public bool lockCameraPosition;

        [Header("Audio")]
        public AudioClip landingAudioClip;
        public AudioClip[] footstepAudioClips;
        [Range(0, 1)] public float footstepAudioVolume = 0.3f;

        private Animator _animator;
        private CharacterController _controller;
        private InputSystem _input;
        private GameObject _mainCamera;
#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private readonly float _terminalVelocity = 53.0f;
        private int _animIDFreeFall;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDMotionSpeed;
        private int _animIDSpeed;
        private float _cinemachineTargetPitch;
        private float _cinemachineTargetYaw;
        private float _fallTimeoutDelta;
        private float _jumpTimeoutDelta;
        private float _rotationVelocity;
        private float _speed;
        private float _verticalVelocity;
        private bool _hasAnimator;
        private bool _crouchInputHeld;
        private bool _isCrouching;

        internal bool IsClimbing;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (_mainCamera == null) _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        private void Start()
        {
            _cinemachineTargetYaw = playerCamera.transform.rotation.eulerAngles.y;
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<InputSystem>();
            _hasAnimator = TryGetComponent(out _animator);
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("PlayerInput component is missing.");
#endif

            _jumpTimeoutDelta = jumpTimeout;
            _fallTimeoutDelta = fallTimeout;

            AssignAnimationIDs();
        }

        private void Update()
        {
            if (!IsOwner) return;

            _hasAnimator = TryGetComponent(out _animator);

            GroundedCheck();
            JumpAndGravity();
            Move();
        }

        private void LateUpdate()
        {
            if (!IsOwner) return;
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            var spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
            grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

            if (_hasAnimator) _animator.SetBool(_animIDGrounded, grounded);
        }

        private void CameraRotation()
        {
            if (playerCamera == null) return;

            if (_input.look.sqrMagnitude >= Threshold && !lockCameraPosition)
            {
                var deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

            playerCamera.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + cameraAngleOverride, _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            var targetSpeed = DetermineTargetSpeed();
            AdjustSpeed(targetSpeed);

            var inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero) RotatePlayer(inputDirection);

            ApplyMovement();
            HandleClimbing();
            HandleCrouching();
            UpdateAnimator();
        }

        private float DetermineTargetSpeed()
        {
            if (_input.move == Vector2.zero) return 0.0f;

            if (_input.sprint) return sprintSpeed;

            return _isCrouching ? crouchSpeed : moveSpeed;
        }

        private void AdjustSpeed(float targetSpeed)
        {
            var currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            var speedOffset = 0.1f;
            var inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (Mathf.Abs(currentHorizontalSpeed - targetSpeed) > speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }
        }

        private void RotatePlayer(Vector3 inputDirection)
        {
            var targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            var rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref _rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        private void ApplyMovement()
        {
            var targetDirection = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f) * Vector3.forward;
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void HandleClimbing()
        {
            if (IsClimbing)
            {
                var climbDirection = new Vector3(0, _input.move.y * climbSpeed, 0);
                _controller.Move(climbDirection * Time.deltaTime);

                if (_controller.isGrounded && _input.move.y > 0)
                {
                    IsClimbing = false;
                    _controller.slopeLimit = 45.0f;
                }
            }
        }

        private void HandleCrouching()
        {
            if (_input.crouch)
            {
                _isCrouching = !_isCrouching;
                _controller.height = _isCrouching ? 1.0f : 2.0f;
            }
        }

        private void UpdateAnimator()
        {
            if (!_hasAnimator) return;

            var inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
            _animator.SetFloat(_animIDSpeed, _speed);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }

        private void JumpAndGravity()
        {
            if (_input == null) return;

            if (grounded) Grounded();
            else Jump();

            if (_verticalVelocity < _terminalVelocity) _verticalVelocity += gravity * Time.deltaTime;
        }

        private void Jump()
        {
            _jumpTimeoutDelta = jumpTimeout;

            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                if (_hasAnimator) _animator.SetBool(_animIDFreeFall, true);
            }

            _input.jump = false;
        }

        private void Grounded()
        {
            _fallTimeoutDelta = fallTimeout;

            if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;

            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (_hasAnimator) _animator.SetBool(_animIDJump, true);
            }

            if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        public void ReceiveInput(Vector2 move, Vector2 look, bool jump, bool sprint, bool crouch)
        {
            _input.move = move;
            _input.look = look;
            _input.jump = jump;
            _input.sprint = sprint;
            _input.crouch = crouch;
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
                if (footstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, footstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(footstepAudioClips[index], transform.TransformPoint(_controller.center), footstepAudioVolume);
                }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
                AudioSource.PlayClipAtPoint(landingAudioClip, transform.TransformPoint(_controller.center), footstepAudioVolume);
        }
    }
}
