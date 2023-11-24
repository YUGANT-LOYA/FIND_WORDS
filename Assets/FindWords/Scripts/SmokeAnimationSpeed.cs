using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YugantLoyaLibrary.FindWords
{
    [RequireComponent(typeof(Animator))]
    public class SmokeAnimationSpeed : MonoBehaviour
    {
        private Animator _smokeAnimator;
        public float animationSpeed = 1;
        [HideInInspector] public float defaultAnimationSpeed;
        private bool _isSmokeAnimatorNotNull;

        private void Awake()
        {
            _smokeAnimator = GetComponent<Animator>();
        }

        private void Start()
        {
            _smokeAnimator.speed = animationSpeed;
            defaultAnimationSpeed = animationSpeed;
        }

        public float TotalAnimationTime()
        {
            float totalTime = _smokeAnimator.GetCurrentAnimatorClipInfo(0).Length;
            Debug.Log("Total Animation Clip Time : " + totalTime);
            return totalTime;
        }

        public void ChangeAnimationSpeed(float speed)
        {
            if (_smokeAnimator != null)
            {
                _smokeAnimator.speed = speed;
            }
        }
    }
}