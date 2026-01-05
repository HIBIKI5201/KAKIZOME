using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Master.Entities
{
    public struct GlobalState : IComponentData, INativeDisposable
    {
        public GlobalState(int count, float phase1Duration)
        {
            Count = count;
            Phase1Duration = phase1Duration;

            PhaseArray = new NativeArray<int>(count, Allocator.Persistent);
        }

        public readonly int Count;
        public readonly float Phase1Duration;

        public readonly NativeArray<int> PhaseArray;

        public JobHandle Dispose(JobHandle inputDeps)
        {
            if (PhaseArray.IsCreated)
            {
                return PhaseArray.Dispose(inputDeps);
            }

            return inputDeps;
        }

        // 即時解放
        public void Dispose()
        {
            if (PhaseArray.IsCreated)
            {
                PhaseArray.Dispose();
            }
        }
    }
}
