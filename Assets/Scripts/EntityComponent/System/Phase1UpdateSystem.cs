using Master.Configs;
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

        public void OnDestroy(ref SystemState state)
        {
            _phase1EntityQuery.Dispose();
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

            if (timer.Timer < timer.ElapsedTime)
            {
                particle.Phase = 2;
                ECB.RemoveComponent<Phase1TimerEntity>(entityIndex, entity);

                float d = Phase2Configs.Duration;
                if (particle.Index % 3 != 0) { d *= 3; }

                var newTimer = new Phase2TimerEntity(d);
                ECB.AddComponent(entityIndex, entity, newTimer);
            }
        }
    }
}
