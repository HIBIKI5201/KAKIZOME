using System;
using UnityEngine;

namespace Master.Configs
{
    [Serializable]
    public struct GlobalConfigs
    {
        public float ParticleSpeed => _particleSpeed;
        public Vector3 CenterPosition => _centerPosition;

        [SerializeField]
        private float _particleSpeed;
        [SerializeField]
        private Vector3 _centerPosition;
    }
}