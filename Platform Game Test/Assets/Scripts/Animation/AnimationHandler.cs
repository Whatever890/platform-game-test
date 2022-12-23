using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
    
namespace Gameplay.Animation
{
    public class AnimationHandler : MonoBehaviour
    {
        [SerializeField] private SkeletonAnimation _skeletonAnimation;

		public List<StateNameToAnimationReference> statesAndAnimations = new List<StateNameToAnimationReference>();
		public List<AnimationTransition> transitions = new List<AnimationTransition>();

		[System.Serializable]
		public class StateNameToAnimationReference
        {
			public string stateName;
			public AnimationReferenceAsset animation;
		}

		[System.Serializable]
		public class AnimationTransition
        {
			public AnimationReferenceAsset from;
			public AnimationReferenceAsset to;
			public AnimationReferenceAsset transition;
		}
        
		public Spine.Animation TargetAnimation { get; private set; }

        /// <summary>
        /// Sets the horizontal flip state of the skeleton based on a nonzero float.
        /// If negative, the skeleton is flipped. If positive, the skeleton is not flipped.
        /// </summary>
        public void SetFlip(float horizontal)
        {
			if (horizontal != 0)
            {
				_skeletonAnimation.Skeleton.ScaleX = horizontal > 0 ? 1f : -1f;
			}
		}

		/// <summary>Plays an animation based on the state name.</summary>
		public void PlayAnimationForState(string stateName, int layerIndex)
        {
            int stateHash = StringToHash(stateName);
			var foundAnimation = GetAnimationForState(stateHash);
			if (foundAnimation == null)
				return;

			PlayNewAnimation(foundAnimation, layerIndex);
		}

		/// <summary>Gets a Spine Animation based on the hash of the state name.</summary>
		private Spine.Animation GetAnimationForState(int stateNameHash)
        {
			var foundState = statesAndAnimations.Find(entry => StringToHash(entry.stateName) == stateNameHash);
			return (foundState == null) ? null : foundState.animation;
		}

		/// <summary>
        /// Play an animation. If a transition animation is defined, the transition is played
        /// before the target animation being passed.
        /// </summary>
		public void PlayNewAnimation(Spine.Animation target, int layerIndex)
        {
			Spine.Animation transition = null;
			Spine.Animation current = null;

			current = GetCurrentAnimation(layerIndex);
			if (current != null)
				transition = TryGetTransition(current, target);

			if (transition != null)
            {
				_skeletonAnimation.AnimationState.SetAnimation(layerIndex, transition, false);
				_skeletonAnimation.AnimationState.AddAnimation(layerIndex, target, true, 0f);
			}
            else
            {
				_skeletonAnimation.AnimationState.SetAnimation(layerIndex, target, true);
			}

			this.TargetAnimation = target;
		}

		Spine.Animation TryGetTransition(Spine.Animation from, Spine.Animation to)
        {
			foreach (var transition in transitions) {
				if (transition.from.Animation == from && transition.to.Animation == to) {
					return transition.transition.Animation;
				}
			}
			return null;
		}

		Spine.Animation GetCurrentAnimation(int layerIndex)
        {
			var currentTrackEntry = _skeletonAnimation.AnimationState.GetCurrent(layerIndex);
			return (currentTrackEntry != null) ? currentTrackEntry.Animation : null;
		}

		int StringToHash(string s)
        {
			return Animator.StringToHash(s);
		}
    }
}