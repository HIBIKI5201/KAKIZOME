using System;
using UnityEngine;

namespace Master.Configs
{
    [Serializable]
    public struct PhaseFinalConfigs
    {
        public float StopDistance => _stopDistance;
        public float FollowGain => _followGain;

        [SerializeField]
        private float _stopDistance;
        [SerializeField, Tooltip("文字への追従ゲイン")]
        private float _followGain;
    }
}
