using Master.Configs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Master.Entities
{
    public struct GlobalState : IComponentData, INativeDisposable
    {
        public GlobalState(int count, int kernelValue,
            Phase1Configs phase1Configs, Phase2Configs phase2Configs)
        {
            Count = count;
            KernelValue = kernelValue;
            Phase1Configs = phase1Configs;
            Phase2Configs = phase2Configs;

            PhaseCountArray = new NativeArray<int>(count, Allocator.Persistent);
        }

        public readonly int Count;
        public readonly int KernelValue;
        public readonly Phase1Configs Phase1Configs;
        public readonly Phase2Configs Phase2Configs;

        public NativeArray<int> PhaseCountArray;

        public JobHandle Dispose(JobHandle inputDeps)
        {
            if (PhaseCountArray.IsCreated)
            {
                return PhaseCountArray.Dispose(inputDeps);
            }

            return inputDeps;
        }

        // 即時解放
        public void Dispose()
        {
            if (PhaseCountArray.IsCreated)
            {
                PhaseCountArray.Dispose();
            }
        }
    }
}
