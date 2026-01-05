using Master.Modules;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
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
            NativeArray<int> phaseArray = new(entityCount, Allocator.TempJob);


            // ジョブの設定とスケジューリング。
            PhaseUpdateJob job = new()
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                PhaseOutput = phaseArray
            };

            state.Dependency = job.Schedule(_particleEntityQuery, state.Dependency);
            state.Dependency.Complete();

            // フェーズカウントの集計。
            globalState.PhaseCountArray.FillArray(0);
            for (int i = 0; i < entityCount; i++)
            {
                int phase = phaseArray[i];
                globalState.PhaseCountArray[phase]++;
            }
        }

        private EntityQuery _particleEntityQuery;
    }

    [BurstCompile]
    public partial struct PhaseUpdateJob : IJobEntity
    {
        public float DeltaTime;

        public NativeArray<int> PhaseOutput;

        void Execute(
            [EntityIndexInQuery] int entityIndex,
            ref ParticleEntity particle,
            ref Phase1TimerEntity phase1Timer)
        {
            phase1Timer.ElapsedTime += DeltaTime;

            if (phase1Timer.Timer < phase1Timer.ElapsedTime)
            {
                particle.Phase = 1;
            }

            PhaseOutput[entityIndex] = particle.Phase;
        }
    }
}