using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Master.Entities
{
    public struct GlobalState : IComponentData, INativeDisposable
    {
        public GlobalState(int count, int kernelValue, float phase1Duration)
        {
            Count = count;
            KernelValue = kernelValue;
            Phase1Duration = phase1Duration;

            PhaseCountArray = new NativeArray<int>(count, Allocator.Persistent);
        }

        public readonly int Count;
        public readonly int KernelValue;
        public readonly float Phase1Duration;

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
