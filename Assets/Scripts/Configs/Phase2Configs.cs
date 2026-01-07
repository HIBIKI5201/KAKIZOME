using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Master.Configs
{
    [Serializable]
    public struct Phase2Configs
    {
        public float Duration => _duration;
        public float TorusMaxRadius => _torusRadius.y;
        public float TorusMinRadius => _torusRadius.x;

        [SerializeField, Tooltip("フェーズ２の時間")]
        private float _duration;
        [SerializeField, Tooltip("円環の半径"), MinMaxRangeSlider(0, 30)]
        private Vector2 _torusRadius;
    }
}