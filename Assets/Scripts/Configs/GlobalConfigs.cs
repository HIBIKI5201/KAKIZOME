using System;
using UnityEngine;

namespace Master.Configs
{
    [Serializable]
    public struct GlobalConfigs
    {
        public Vector3 CenterPosition => _centerPosition;

        [SerializeField]
        private Vector3 _centerPosition;
    }
}