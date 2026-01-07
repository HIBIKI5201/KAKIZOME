using Master.Configs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Master.Entities
{
    public struct GlobalState : IComponentData
    {
        public GlobalState(int count, int kernelValue,
            Phase1Configs phase1Configs, Phase2Configs phase2Configs)
        {
            Count = count;
            KernelValue = kernelValue;
            Phase1Configs = phase1Configs;
            Phase2Configs = phase2Configs;
        }

        public readonly int Count;
        public readonly int KernelValue;
        public readonly Phase1Configs Phase1Configs;
        public readonly Phase2Configs Phase2Configs;
    }
}
