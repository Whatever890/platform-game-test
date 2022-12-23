using UnityEngine;

namespace Gameplay.Camera
{
    public class ConstrainedCamera : MonoBehaviour
    {
        [SerializeField] private Transform _target;
		[SerializeField] private Vector3 _offset;
		[SerializeField] private Vector3 _min;
		[SerializeField] private Vector3 _max;
		[SerializeField] private float _smoothing = 5f;

		void LateUpdate()
        {
			Vector3 goalPoint = _target.position + _offset;
			goalPoint.x = Mathf.Clamp(goalPoint.x, _min.x, _max.x);
			goalPoint.y = Mathf.Clamp(goalPoint.y, _min.y, _max.y);
			goalPoint.z = Mathf.Clamp(goalPoint.z, _min.z, _max.z);

			transform.position = Vector3.Lerp(transform.position, goalPoint, _smoothing * Time.deltaTime);
		}
    }
}