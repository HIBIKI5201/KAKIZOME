using UnityEngine;

namespace Master.Configs
{
    public struct Phase0Configs
    {
        public float InitialRadius => _initialRadius;

        [SerializeField]
        private float _initialRadius;
    }
}
