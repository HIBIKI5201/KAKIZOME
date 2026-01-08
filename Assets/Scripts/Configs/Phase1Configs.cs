using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Master.Configs
{
    [Serializable]
    public struct Phase1Configs
    {
        public float Duration => _duration;
        public float DurationRangeMax => _durationRange.y;
        public float DurationRangeMin => _durationRange.x;
        public float Accel => _phase1Accel;

        [SerializeField, Tooltip("フェーズ１の長さ")]
        private float _duration;
        [SerializeField, Tooltip("フェーズ１の長さの幅"), MinMaxRangeSlider(-10, 10)]
        private Vector2 _durationRange;
        [SerializeField, Tooltip("放射拡散の加速度")]
        private float _phase1Accel;
    }
}