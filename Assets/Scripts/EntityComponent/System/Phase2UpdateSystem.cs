using Master.Utility;
using Unity.Burst;
using Unity.Entities;

namespace Master.Entities
{
    /// <summary>
    /// フェーズ2から3への移行を処理するシステム。
    /// </summary>
    [DisableAutoCreation]
    [UpdateInGroup(typeof(ParticleSystemGroup))]
    [UpdateAfter(typeof(Phase1UpdateSystem))]
    public partial struct Phase2UpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            _phase2EntityQuery = state.GetEntityQuery(
                ComponentType.ReadWrite<ParticleEntity>(),
                ComponentType.ReadWrite<Phase2TimerEntity>()
            );
            state.RequireForUpdate<GlobalState>();
            state.RequireForUpdate(_phase2EntityQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

            var job = new Phase2UpdateJob
            {
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            state.Dependency = job.ScheduleParallel(_phase2EntityQuery, state.Dependency);
        }

        private EntityQuery _phase2EntityQuery;
    }

    [BurstCompile]
    public partial struct Phase2UpdateJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;

        void Execute(
            [EntityIndexInQuery] int entityIndexInQuery,
            Entity entity,
            ref ParticleEntity particle,
            ref Phase2TimerEntity timer)
        {
            timer.ElapsedTime += DeltaTime;

            if (timer.ElapsedTime <= timer.Timer) { return; }

            particle.Phase = 
                ParticleAffiliationUtility.GetAffiliationByIndex(particle.Index, particle.Phase);

            ECB.RemoveComponent<Phase2TimerEntity>(entityIndexInQuery, entity);
        }
    }
}
