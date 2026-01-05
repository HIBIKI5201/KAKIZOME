using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Master.Entities
{
    public struct GlobalState : IComponentData, INativeDisposable
    {
        public GlobalState(int count, int kernelValue)
        {
            KernelValue = kernelValue;

            PhaseCountArray = new NativeArray<int>(count, Allocator.Persistent);
        }

        public readonly int KernelValue;

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
