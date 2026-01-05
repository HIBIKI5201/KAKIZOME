using Master.Configs;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Master.Entities
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(ParticleSystemGroup))]
    public partial struct ParticleInitializeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GlobalState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            GlobalState globalState = SystemAPI.GetSingleton<GlobalState>();
            Phase1Configs configs = globalState.Phase1Configs;
            Random rnd = new Random(19320616);

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            for (int i = 0; i < globalState.Count; i++)
            {
                Entity entity = ecb.CreateEntity();
                ecb.AddComponent(entity, new ParticleEntity(i));
                
                float r = rnd.NextFloat();
                float d = configs.Duration + math.lerp(configs.DurationRange.x, configs.DurationRange.y, r);
                ecb.AddComponent(entity, new Phase1TimerEntity(d));
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            // 初期化が完了したら、このシステムを無効化。
            state.Enabled = false;
        }
    }
}