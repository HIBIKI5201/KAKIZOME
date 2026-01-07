using Master.Configs;
using Master.Utility;
using NUnit.Framework.Internal.Filters;
using Unity.Burst;
using Unity.Entities;

namespace Master.Entities
{
    /// <summary>
    /// フェーズ1から2への移行を処理するシステム。
    /// </summary>
    [DisableAutoCreation]
    [UpdateInGroup(typeof(ParticleSystemGroup))]
    [UpdateAfter(typeof(ParticleInitializeSystem))]
    public partial struct Phase1UpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            _phase1EntityQuery = state.GetEntityQuery(
                ComponentType.ReadWrite<ParticleEntity>(),
                ComponentType.ReadWrite<Phase1TimerEntity>()
            );
            state.RequireForUpdate<GlobalState>();
            state.RequireForUpdate(_phase1EntityQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            
            var job = new Phase1UpdateJob
            {
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                Phase2Configs = SystemAPI.GetSingleton<GlobalState>().Phase2Configs,
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            
            state.Dependency = job.ScheduleParallel(_phase1EntityQuery, state.Dependency);
        }

        private EntityQuery _phase1EntityQuery;
    }

    [BurstCompile]
    public partial struct Phase1UpdateJob : IJobEntity
    {
        public float DeltaTime;
        public Phase2Configs Phase2Configs;
        public EntityCommandBuffer.ParallelWriter ECB;

        void Execute(
            [EntityIndexInQuery] int entityIndex,
            Entity entity,
            ref ParticleEntity particle,
            ref Phase1TimerEntity timer)
        {
            timer.ElapsedTime += DeltaTime;

            if (timer.ElapsedTime <= timer.Timer) { return; }

            ECB.RemoveComponent<Phase1TimerEntity>(entityIndex, entity);

            particle.Phase = 2;

            int affiliation = ParticleAffiliationUtility.GetAffiliationByIndex(particle.Index, particle.Phase);
            float duration = 0;
            if (affiliation == 3) { duration = Phase2Configs.DurationToPhase3; }
            else if (affiliation == 4) { duration = Phase2Configs.DurationToPhaseFinal; }

                var newTimer = new Phase2TimerEntity(duration);
            ECB.AddComponent(entityIndex, entity, newTimer);
        }
    }
}
