using System;
using UnityEngine;

namespace Master.Configs
{
    [Serializable]
    public struct PhaseConfigRepository
    {
        public GlobalConfigs GlobalConfigs => _globalConfigs;
        public Phase1Configs Phase1Configs => _phase1Configs;
        public Phase2Configs Phase2Configs => _phase2Configs;
        public PhaseFinalConfigs PhaseFinalConfigs => _phaseFinalConfigs;

        [SerializeField]
        private GlobalConfigs _globalConfigs;
        [SerializeField]
        private Phase1Configs _phase1Configs;
        [SerializeField]
        private Phase2Configs _phase2Configs;
        [SerializeField]
        private PhaseFinalConfigs _phaseFinalConfigs;
    }
}
