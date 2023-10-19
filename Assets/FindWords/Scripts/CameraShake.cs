using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

namespace YugantLibrary.ParkingOrderGame
{
    public class CameraShake : MonoBehaviour
    {
        Vector3 defaultPos;
        [Range(0f, 2f)] [SerializeField] float shakeDuration = 0.4f;
        [Range(0f, 1f)] [SerializeField] float shakeStrength = 0.25f;
        [Range(0, 90)] [SerializeField] int shakeRandomness = 15;

        private void Start()
        {
            defaultPos = transform.position;
        }

        [Button]
        public void ShakeCamera(Vector3 defaultCamPos = default(Vector3))
        {
            if (defaultCamPos == default(Vector3))
            {
                defaultCamPos = defaultPos;
            }

            transform.DOShakePosition(shakeDuration, shakeStrength, shakeRandomness).OnComplete(() =>
            {
                transform.position = defaultCamPos;
            });
        }
    }
}