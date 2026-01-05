using Unity.Entities;

namespace Master.Entities
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(ParticleSystemGroup))]
    public partial struct ParticleInitializeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ParticleInitializeEntity>();
        }
        public void OnUpdate(ref SystemState state)
        {
            ParticleInitializeEntity globalState = SystemAPI.GetSingleton<ParticleInitializeEntity>();

            for (int i = 0; i < globalState.Count; i++)
            {
                Entity entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, new ParticleEntity { Phase = 0 });

                state.EntityManager.AddComponentData(entity, new Phase1TimerEntity(globalState.Phase1Duration));
            }

            // 初期化が完了したら、このシステムを無効化。
            state.Enabled = false;
        }
    }
}