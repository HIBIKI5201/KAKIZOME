using System;
using UnityEngine;

namespace Master.Configs
{
    [Serializable]
    public struct PhaseConfigRepository
    {
        public GlobalConfigs GlobalConfigs => _globalConfigs;
        public Phase0Configs Phase0Configs => _phase0Configs;
        public Phase1Configs Phase1Configs => _phase1Configs;
        public Phase2Configs Phase2Configs => _phase2Configs;
        public Phase3Configs Phase3Configs => _phase3Configs;
        public PhaseFinalConfigs PhaseFinalConfigs => _phaseFinalConfigs;

        [SerializeField]
        private GlobalConfigs _globalConfigs;
        [SerializeField]
        private Phase0Configs _phase0Configs;
        [SerializeField]
        private Phase1Configs _phase1Configs;
        [SerializeField]
        private Phase2Configs _phase2Configs;
        [SerializeField]
        private Phase3Configs _phase3Configs;
        [SerializeField]
        private PhaseFinalConfigs _phaseFinalConfigs;
    }
}
