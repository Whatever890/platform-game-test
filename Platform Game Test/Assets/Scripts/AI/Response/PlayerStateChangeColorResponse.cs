using UnityEngine;
using Gameplay.Player;

namespace Gameplay.AI
{
    public class PlayerStateChangeColorResponse : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Renderer _renderer;
        
        [Header("Response Setup")]
        [SerializeField] private PlayerState _responseState;
        [SerializeField] private Color _responseColor;

        private Color _initialColor = Color.red;
        private bool _responseActive = false;

        private void Start()
        {
            _playerController.OnStateChanged += CheckResponse;
            _initialColor = _renderer.sharedMaterial.color;
        }

        /// <summary>
        /// Checks if the new state of the Player is the one expected. If yes, does color
        /// response, otherwise sets the color back to the initial one.
        /// </summary>
        private void CheckResponse(PlayerState state)
        {
            if (!_responseActive && state == _responseState)
            {
                SetColor(_responseColor);
                _responseActive = true;
            }
            else if (_responseActive && state != _responseState)
            {
                SetColor(_initialColor);
                _responseActive = false;
            }
        }

        /// <summary> Sets material color using MaterialPropertyBlock. </summary>
        private void SetColor(Color color)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", color);
            _renderer.SetPropertyBlock(propBlock);
        }
    }
}