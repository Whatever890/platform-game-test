using UnityEngine;
using UnityEngine.Events;
using Gameplay.Animation;

namespace Gameplay.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
		[SerializeField] private CharacterController _controller;

		[Header("Controls")]
		[SerializeField] private string _xAxis = "Horizontal";
		[SerializeField] private string _jumpButton = "Jump";

		[Header("Moving")]
		[SerializeField] private float _walkSpeed = 8f;
		[SerializeField] private float _runSpeed = 15f;

		[Header("Jumping")]
		[SerializeField] private float _jumpStrength = 35f;
		[SerializeField] private float _gravityScale = 6.6f;

		[Header("Sliding")]
		[SerializeField] private float _slideSpeed = 50f;
        [SerializeField] private float _inertiaDuration = 0.5f;
        [SerializeField] private LayerMask _groundLayer;

		[Header("Animation")]
		[SerializeField] private AnimationHandler _animationHandler;

		private Vector2 _input = default(Vector2);
		private Vector3 _velocity = default(Vector3);

        private bool _isGrounded = false;
		private bool _wasGrounded = false;

        private bool _isSliding = false;
        private bool _wasSliding = false;
        private float _inertiaEndTime;
        private float _inertiaVelocity;

		private PlayerState _previousState;
        private PlayerState _currentState;

		public event System.Action OnJump, OnLand;
        public event System.Action<PlayerState> OnStateChanged;

		void Update()
        {
            // Check user input and surrounding and set move velocity
			CheckGround();
            CheckSlope();
            CheckJump();
            CheckMove();
            CheckInertia();

            // Apply final velocity
            ApplyMovement();
            // Set state and visualize
            SetState();
		}

        /// <summary>
        /// Checks whether the Player is grounded and whether it just landed.
        /// Applies gravity if the Player is in hte air.
        /// </summary>
        private void CheckGround()
        {
            _isGrounded = _controller.isGrounded;
            if (!_wasGrounded)
            {
                if (!_isGrounded)
                {
                    _velocity.y += Physics.gravity.y * _gravityScale * Time.deltaTime;
                }
                else
                {
                    OnLand?.Invoke();
                }
            }
            _wasGrounded = _isGrounded;
        }

        /// <summary>
        /// Checks whether the Player is on an unwalkable slope. If so, apply
        /// velocity along the slope angle.
        /// </summary>
        private void CheckSlope()
        {
            if (!_isGrounded)
                return;

            RaycastHit hit;
            Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, Mathf.Infinity, _groundLayer);
            Vector3 groundNormal = hit.normal;
            float angle = Vector3.SignedAngle(groundNormal, Vector3.up, Vector3.forward);
            _isSliding = Mathf.Abs(angle) > _controller.slopeLimit;

            if (_isSliding)
            {
                Vector3 slideVelocity = Vector3.ProjectOnPlane(new Vector3(0f, -_slideSpeed, 0f), groundNormal);
                _velocity.x = slideVelocity.x;
                _velocity.y = slideVelocity.y;
                _inertiaVelocity = slideVelocity.x;
            }
        }

        /// <summary>
        /// Checks whether the jump button was pressed. If so and the player is grounded, apply
        /// Vertical jump velocity.
        /// </summary>
        private void CheckJump()
        {
            if (!_isGrounded || _isSliding)
                return;

            if (Input.GetButtonDown(_jumpButton))
            {
                _velocity.y = _jumpStrength;
                OnJump?.Invoke();
            }
        }

        /// <summary>
        /// Checks whether Horizontal axis is input. If so and the player is grounded, apply
        /// Horizontal velocity.
        /// </summary>
        private void CheckMove()
        {
            if (_isSliding)
                return;

            _velocity.x = 0;
            _input.x = Input.GetAxis(_xAxis);
			if (_input.x != 0)
            {
                _velocity.x = Mathf.Abs(_input.x) > 0.6f ? _runSpeed : _walkSpeed;
                _velocity.x *= Mathf.Sign(_input.x);
            }
        }

        /// <summary>
        /// Checks whether the Player just finished sliding. If so, apply Horizontal velocity inertia after sliding.
        /// </summary>
        private void CheckInertia()
        {
            if (_wasSliding && !_isSliding)
            {
                _inertiaEndTime = Time.time + _inertiaDuration;
            }
            
            float inertiaLeftTime = _inertiaEndTime - Time.time;
            if (inertiaLeftTime > 0f)
            {
                float inertiaTimePoint = Mathf.InverseLerp(0f, _inertiaDuration, inertiaLeftTime);
                float inertia = Mathf.Lerp(0f, _inertiaVelocity, inertiaTimePoint);
                _velocity.x += inertia;
            }
            _wasSliding = _isSliding;
        }

        /// <summary> Applies final velocity to the Player. </summary>
        private void ApplyMovement()
        {
            _controller.Move(_velocity * Time.deltaTime);
        }

        /// <summary> Sets final state based on the movement data. </summary>
        private void SetState()
        {
            if (_isSliding)
            {
                _currentState = PlayerState.Slide;
            }
            else if (_isGrounded)
            {
                if (_velocity.x == 0)
                    _currentState = PlayerState.Idle;
                else
                    _currentState = Mathf.Abs(_velocity.x) > Mathf.Lerp(_walkSpeed, _runSpeed, 0.5f) ? PlayerState.Run : PlayerState.Walk;
			}
            else
            {
				_currentState = PlayerState.Jump;
			}

			bool stateChanged = _previousState != _currentState;
			_previousState = _currentState;
			if (stateChanged)
            {
				HandleStateChange();
                OnStateChanged?.Invoke(_currentState);
            }

			if (_velocity.x != 0)
            {
				_animationHandler.SetFlip(_velocity.x);
            }
        }

        /// <summary> If the state has changed, gets its name and sends over to AnimationHandler. </summary>
        void HandleStateChange()
        {
			string stateName = null;
			switch (_currentState)
            {
                case PlayerState.Idle:
                    stateName = "idle";
                    break;
                case PlayerState.Walk:
                    stateName = "walk";
                    break;
                case PlayerState.Run:
                    stateName = "run";
                    break;
                case PlayerState.Jump:
                    stateName = "jump";
                    break;
                case PlayerState.Slide:
                    stateName = "slide";
                    break;
                default:
                    break;
			}

			_animationHandler.PlayAnimationForState(stateName, 0);
		}
    }
}