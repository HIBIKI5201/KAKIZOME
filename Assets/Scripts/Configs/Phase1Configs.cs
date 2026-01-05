using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Master.Configs
{
    [Serializable]
    public struct Phase1Configs
    {
        public float Duration => _duration;
        public Vector2 DurationRange => _durationRange;
        public float RotationRadius => _rotationRadius;

        [SerializeField, Tooltip("フェーズ１の長さ")]
        private float _duration;
        [SerializeField, Tooltip("フェーズ１の長さの幅"), MinMaxRangeSlider(-10, 10)]
        private Vector2 _durationRange;

        [SerializeField, Tooltip("回転する半径")]
        private float _rotationRadius;
    }
}