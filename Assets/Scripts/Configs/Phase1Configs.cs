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

        [SerializeField, Tooltip("フェーズ１の長さ")]
        private float _duration;
        [SerializeField, Tooltip("フェーズ１の長さの幅"), MinMaxRangeSlider(-10, 10)]
        private Vector2 _durationRange;
    }
}