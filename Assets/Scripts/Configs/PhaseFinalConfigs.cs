using System;
using UnityEngine;

namespace Master.Configs
{
    [Serializable]
    public struct PhaseFinalConfigs
    {
        public float StopDistance => _stopDistance;

        [SerializeField]
        private float _stopDistance;
    }
}
