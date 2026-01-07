using Master.Modules;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace Master.Entities
{
    /// <summary>
    /// 各パーティクルのフェーズ状態をGPUバッファに同期するシステム。
    /// </summary>
    [DisableAutoCreation]
    [UpdateInGroup(typeof(ParticleSystemGroup))]
    [UpdateAfter(typeof(Phase2UpdateSystem))]
    public partial struct ParticlePhaseSyncSystem : ISystem
    {
        private EntityQuery _particleQuery;
        private EntityQuery _phaseCountQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GlobalState>();
            _particleQuery = state.GetEntityQuery(ComponentType.ReadOnly<ParticleEntity>());
            _phaseCountQuery = state.GetEntityQuery(ComponentType.ReadWrite<PhaseCountEntity>());
        }

        public void OnUpdate(ref SystemState state)
        {
            const int BATCH_COUNT = 64;

            GlobalState globalState = SystemAPI.GetSingleton<GlobalState>();
            int particleCount = globalState.Count;
            int kernelValue = globalState.KernelValue;
            if (particleCount == 0) { return; }

            // ジョブ実行。
            var particles = new NativeArray<ParticleEntity>(particleCount, Allocator.TempJob);
            var counts = new NativeArray<int>(kernelValue, Allocator.TempJob);
            var starts = new NativeArray<int>(kernelValue, Allocator.TempJob);
            var sortedIndices = new NativeArray<uint>(particleCount, Allocator.TempJob);

            int workerCount = JobsUtility.MaxJobThreadCount;
            var threadCounts = new NativeArray<int>(workerCount * kernelValue, Allocator.TempJob);
            var threadOffsets = new NativeArray<int>(threadCounts.Length, Allocator.TempJob);

            CreateParticleArrayJob cpaJob = new()
            {
                Particles = particles,
            };
            JobHandle cpaHandle = cpaJob.ScheduleParallel(_particleQuery, state.Dependency);

            ThreadLocalCountJob tlcJob = new()
            {
                KernelCount = kernelValue,
                Particles = particles,
                ThreadCounts = threadCounts,
            };
            JobHandle tlcHandle = tlcJob.Schedule(particleCount, BATCH_COUNT, cpaHandle);

            MergeCountsJob mcJob = new()
            {
                KernelCount = kernelValue,
                ThreadCounts = threadCounts,
                Counts = counts,
                WorkerCount = workerCount
            };
            JobHandle mcHandle = mcJob.Schedule(tlcHandle);

            PrefixSumJob psJob = new()
            {
                Counts = counts,
                Starts = starts,
            };
            JobHandle psHandle = psJob.Schedule(mcHandle);

            CalcThreadCountsJob ctcJob = new()
            {
                kernelValue = kernelValue,
                starts = starts,
                threadCounts = threadCounts,
                threadOffsets = threadOffsets,
                workerCount = workerCount
            };
            JobHandle ctcHandle = ctcJob.Schedule(psHandle);

            ThreadLocalScatterJob tlsJob = new()
            {
                KernelCount = kernelValue,
                Particles = particles,
                SortedIndices = sortedIndices,
                ThreadOffsets = threadOffsets,
            };
            JobHandle tlsHandle = tlsJob.Schedule(particleCount, BATCH_COUNT, ctcHandle);


            state.Dependency = tlsHandle;
            state.Dependency.Complete();
            // ジョブ結果をバッファと配列に格納する。

            IGraphicBufferContainer container = GPUBufferContainerLocator.Get();
            var entity = _phaseCountQuery.GetSingletonEntity();
            var phaseCountBuffer = state.EntityManager.GetBuffer<PhaseCountEntity>(entity);

            for (int i = 0; i < kernelValue; i++)
            {
                int count = counts[i];
                if (count > 0)
                {
                    var slice = sortedIndices.GetSubArray(starts[i], count);
                    container.PhaseIndicesBuffers[i].SetData(slice);
                }
                else
                {
                    container.PhaseIndicesBuffers[i].SetData(new int[0], 0, 0, 0);
                }
                phaseCountBuffer[i] = new PhaseCountEntity(count);
            }

            // メモリ解放。
            particles.Dispose();
            counts.Dispose();
            starts.Dispose();
            threadCounts.Dispose();
            threadOffsets.Dispose();
            sortedIndices.Dispose();
        }
    }

    [BurstCompile]
    public partial struct CreateParticleArrayJob : IJobEntity
    {
        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<ParticleEntity> Particles;
        public void Execute(in ParticleEntity particle) => Particles[particle.Index] = particle;
    }

    [BurstCompile]
    public struct ThreadLocalCountJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<ParticleEntity> Particles;

        [NativeDisableParallelForRestriction]
        public NativeArray<int> ThreadCounts;

        public int KernelCount;

        public void Execute(int startIndex, int count)
        {
            int threadIndex = JobsUtility.ThreadIndex;
            int end = startIndex + count;

            for (int index = startIndex; index < end; index++)
            {
                int phase = Particles[index].Phase - 1;
                if ((uint)phase >= (uint)KernelCount) continue;

                int offset = threadIndex * KernelCount + phase;
                ThreadCounts[offset]++;
            }
        }
    }

    [BurstCompile]
    public struct MergeCountsJob : IJob
    {
        [ReadOnly]
        public NativeArray<int> ThreadCounts;

        public NativeArray<int> Counts;

        public int KernelCount;
        public int WorkerCount;

        public void Execute()
        {
            for (int k = 0; k < KernelCount; k++)
            {
                int sum = 0;
                for (int t = 0; t < WorkerCount; t++)
                {
                    sum += ThreadCounts[t * KernelCount + k];
                }
                Counts[k] = sum;
            }
        }
    }

    [BurstCompile]
    public partial struct PrefixSumJob : IJob
    {
        [ReadOnly]
        public NativeArray<int> Counts;
        [WriteOnly]
        public NativeArray<int> Starts;

        public void Execute()
        {
            int sum = 0;
            for (int i = 0; i < Counts.Length; i++)
            {
                Starts[i] = sum;
                sum += Counts[i];
            }
        }
    }

    [BurstCompile]
    public struct CalcThreadCountsJob : IJob
    {
        public int workerCount;
        public int kernelValue;
        public NativeArray<int> threadCounts;
        public NativeArray<int> starts;
        public NativeArray<int> threadOffsets;

        public void Execute()
        {
            for (int t = 0; t < workerCount; t++)
            {
                for (int k = 0; k < kernelValue; k++)
                {
                    int baseOffset = starts[k];
                    int localSum = 0;

                    for (int i = 0; i < t; i++)
                    {
                        localSum += threadCounts[i * kernelValue + k];
                    }

                    threadOffsets[t * kernelValue + k] = baseOffset + localSum;
                }
            }
        }
    }

    [BurstCompile]
    public struct ThreadLocalScatterJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<ParticleEntity> Particles;

        [NativeDisableParallelForRestriction]
        public NativeArray<int> ThreadOffsets;

        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<uint> SortedIndices;

        public int KernelCount;

        public void Execute(int startIndex, int count)
        {
            int threadIndex = JobsUtility.ThreadIndex;
            int end = startIndex + count;

            for (int index = startIndex; index < end; index++)
            {
                int phase = Particles[index].Phase - 1;
                if ((uint)phase >= (uint)KernelCount) continue;

                int offsetIndex = threadIndex * KernelCount + phase;
                int writeIndex = ThreadOffsets[offsetIndex]++;

                // 安全保証（開発中は必須）
                if ((uint)writeIndex < (uint)SortedIndices.Length)
                {
                    SortedIndices[writeIndex] = (uint)Particles[index].Index;
                }
            }
        }
    }
}