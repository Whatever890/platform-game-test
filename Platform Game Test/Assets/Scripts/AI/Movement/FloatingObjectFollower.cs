using UnityEngine;

namespace Gameplay.AI
{
    public class FloatingObjectFollower : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private Transform _target;
        [SerializeField] private float _followDistance = 5f;
		[SerializeField] private float _smoothing = 5f;
        [SerializeField] private float _zOffset = 2f;

        [Header("Float")]
        [SerializeField] private float _floatHeight = 6f;
        [SerializeField] private float _floatIdleOffset = 2f;
        [SerializeField] private AnimationCurve _floatIdleCurve;
        [SerializeField] private LayerMask _groundLayer;

        private Vector3 _goalPoint = default(Vector3);

        private float _floatTimePoint = 0.5f;
        private bool _floatingUp = true;
        private float _currentFloatOffset;

        private void Update()
        {
            // Set Goal position based on Target's position and floating point
            _goalPoint = transform.position;
            _goalPoint.x = _target.position.x - _followDistance;
            _goalPoint.y = GetFloatingHeightPoint();
            _goalPoint.z = _target.position.z + _zOffset;

            // Smoothly move to the Goal position
            transform.position = Vector3.Lerp(transform.position, _goalPoint, _smoothing * Time.deltaTime);
        }

        /// <summary>
        /// Gets Y coordinate that imitates the object smoothly floating in the air. It takes ground
        /// into account in order to not avoid going through it.
        /// </summary>
        private float GetFloatingHeightPoint()
        {
            float heightPoint = _goalPoint.y;
            if (_floatingUp)
            {
                if (_floatTimePoint < 1f)
                    _floatTimePoint += Time.deltaTime;
                else
                    _floatingUp = false;
            }
            else
            {
                if (_floatTimePoint > 0f)
                    _floatTimePoint -= Time.deltaTime;
                else
                    _floatingUp = true;
            }
            EvaluateFloatOffset();
            heightPoint = GetFloatHeightAboveGround() + _currentFloatOffset;
            return heightPoint;
        }

        /// <summary> Evaluates floating offset using animation curve. </summary>
        private void EvaluateFloatOffset()
        {
            float curvePoint = _floatIdleCurve.Evaluate(_floatTimePoint);
            _currentFloatOffset = Mathf.Lerp(-_floatIdleOffset, _floatIdleOffset, curvePoint);
        }

        /// <summary>
        /// Gets height that is either _floatHeight overt the target or, if too close to ground,
        /// _floatHeight over the ground.
        ///</summary>
        private float GetFloatHeightAboveGround()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, _floatHeight + _currentFloatOffset - 1f, _groundLayer))
            {
                return hit.point.y + _floatHeight;
            }
            return _target.position.y + _floatHeight;
        }
    }
}