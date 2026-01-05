using Master.Modules;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Rendering;

namespace Master.Entities
{
    /// <summary>
    /// フェーズ更新システム。
    /// 各エンティティのフェーズ状態を更新する役割を担う。
    /// </summary>
    [DisableAutoCreation]
    [UpdateAfter(typeof(ParticleInitializeSystem))]
    [UpdateInGroup(typeof(ParticleSystemGroup))]
    public partial struct PhaseUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            _particleEntityQuery = state.GetEntityQuery(
                ComponentType.ReadWrite<ParticleEntity>(),
                ComponentType.ReadWrite<Phase1TimerEntity>()
            );

            state.RequireForUpdate<GlobalState>();
            state.RequireForUpdate(_particleEntityQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            GlobalState globalState = SystemAPI.GetSingleton<GlobalState>();

            int entityCount = _particleEntityQuery.CalculateEntityCount();
            if (entityCount == 0) { return; }
            NativeArray<uint> phaseIndicesArray = new(entityCount, Allocator.TempJob);

            // ジョブの設定とスケジューリング。
            PhaseUpdateJob job = new()
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                PhaseOutput = phaseIndicesArray
            };

            state.Dependency = job.Schedule(_particleEntityQuery, state.Dependency);
            state.Dependency.Complete();

            // 結果のGPUバッファへの転送。
            IGraphicBufferContainer container = GPUBufferContainerLocator.Get();
            NativeArray<uint> phaseIndices = new(entityCount, Allocator.Temp);
            for (int i = 0; i < globalState.KernelValue; i++)
            {
                int count = 0;
                for (int j = 0; j < entityCount; j++)
                {
                    if (phaseIndicesArray[j] == i)
                    {
                        phaseIndices[count] = (uint)j;
                        count++;
                    }
                }

                container.PhaseIndicesBuffers[i].SetData(phaseIndices);
                globalState.PhaseCountArray[i] = count;
            }

            // メモリの解放。
            phaseIndicesArray.Dispose();
        }

        private EntityQuery _particleEntityQuery;
    }

    [BurstCompile]
    public partial struct PhaseUpdateJob : IJobEntity
    {
        public float DeltaTime;

        public NativeArray<uint> PhaseOutput;

        void Execute(
            [EntityIndexInQuery] int entityIndex,
            ref ParticleEntity particle,
            ref Phase1TimerEntity phase1Timer)
        {
            phase1Timer.ElapsedTime += DeltaTime;

            if (phase1Timer.Timer < phase1Timer.ElapsedTime)
            {
                PhaseOutput[entityIndex] = 1;
            }
            else
            {
                PhaseOutput[entityIndex] = 0;
            }
        }
    }
}