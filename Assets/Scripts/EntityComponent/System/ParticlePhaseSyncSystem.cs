using Master.Modules;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEditor.Search;

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
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GlobalState>();
            _phaseCountQuery = state.GetEntityQuery(ComponentType.ReadWrite<PhaseCountEntity>());
        }

        public void OnUpdate(ref SystemState state)
        {
            GlobalState globalState = SystemAPI.GetSingleton<GlobalState>();
            int particleCount = globalState.Count;
            if (particleCount == 0) { return; }

            // フェーズの情報を配列にコピーする。
            var phaseArray = new NativeArray<int>(particleCount, Allocator.TempJob);
            var initJob = new InitializePhaseArrayJob
            {
                PhaseOutput = phaseArray
            };
            state.Dependency = initJob.Schedule(state.Dependency);
            state.Dependency.Complete();

            // フェーズ情報をバッファに渡す。
            IGraphicBufferContainer container = GPUBufferContainerLocator.Get();
            var phaseIndices = new NativeArray<uint>(particleCount, Allocator.Temp);

            var entity = _phaseCountQuery.GetSingletonEntity();
            DynamicBuffer<PhaseCountEntity> buffer =
                state.EntityManager.GetBuffer<PhaseCountEntity>(entity);
            NativeArray<PhaseCountEntity> phaseCountArray = buffer.AsNativeArray();

            for (int i = 0; i < globalState.KernelValue; i++)
            {
                int count = 0;
                for (int j = 0; j < particleCount; j++)
                {
                    // phaseArrayの値は1始まりなので-1して比較。
                    if (phaseArray[j] - 1 == i)
                    {
                        phaseIndices[count] = (uint)j;
                        count++;
                    }
                }

                container.PhaseIndicesBuffers[i].SetData(phaseIndices, 0, 0, count);
                phaseCountArray[i] = new(count);
            }

            // メモリの解放。
            phaseIndices.Dispose();
            phaseArray.Dispose();
        }

        private EntityQuery _phaseCountQuery;
    }

    [BurstCompile]
    public partial struct InitializePhaseArrayJob : IJobEntity
    {
        [WriteOnly]
        public NativeArray<int> PhaseOutput;

        void Execute(in ParticleEntity particle)
        {
            PhaseOutput[particle.Index] = particle.Phase;
        }
    }
} 