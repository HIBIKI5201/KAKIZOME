using System;
using UnityEngine;

namespace Master.Configs
{
    [Serializable]
    public struct Phase2Configs
    {
        public float Duration => _duration;

        [SerializeField, Tooltip("フェーズ２の時間")]
        private float _duration;
    }
}