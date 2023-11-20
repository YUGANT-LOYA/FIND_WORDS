using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SmokeAnimationSpeed : MonoBehaviour
{
    private Animator _smokeAnimator;
    public float animationSpeed = 1;

    private void Start()
    {
        _smokeAnimator = GetComponent<Animator>();
        _smokeAnimator.speed = animationSpeed;
    }
}