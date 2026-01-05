using Master.Configs;
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
            _phase1EntityQuery = state.GetEntityQuery(
                ComponentType.ReadWrite<ParticleEntity>(),
                ComponentType.ReadWrite<Phase1TimerEntity>()
                );
            _phase2EntityQuery = state.GetEntityQuery(
                ComponentType.ReadWrite<ParticleEntity>(),
                ComponentType.ReadWrite<Phase2TimerEntity>()
                );

            state.RequireForUpdate<GlobalState>();
            state.RequireForUpdate(_phase1EntityQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            GlobalState globalState = SystemAPI.GetSingleton<GlobalState>();
            int particleCount = globalState.Count;

            int phase1EntityCount = _phase1EntityQuery.CalculateEntityCount();
            int phase2EntityCount = _phase2EntityQuery.CalculateEntityCount();
            int sumEntityCount = phase1EntityCount + phase2EntityCount;
            if (sumEntityCount == 0) { return; }
            NativeArray<int> phaseArray = new(particleCount, Allocator.TempJob);
            
            var initJob = new InitializePhaseArrayJob
            {
                PhaseOutput = phaseArray
            };
            state.Dependency = initJob.Schedule(state.Dependency);

            var ecb = SystemAPI
                .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            // ジョブの設定とスケジューリング。
            float delta = SystemAPI.Time.DeltaTime;
            Phase1UpdateJob job = new()
            {
                ECB = ecb.AsParallelWriter(),
                Phase2Configs = globalState.Phase2Configs,
                DeltaTime = delta,
                PhaseOutput = phaseArray
            };
            state.Dependency = job.Schedule(_phase1EntityQuery, state.Dependency);

            state.Dependency.Complete();

            Phase2UpdateJob job2 = new()
            {
                ECB = ecb.AsParallelWriter(),
                DeltaTime = delta,
                PhaseOutput = phaseArray
            };
            state.Dependency = job2.Schedule(_phase2EntityQuery, state.Dependency);

            state.Dependency.Complete();

            // 結果をGPUバッファとGlobalStateへ転送。
            IGraphicBufferContainer container = GPUBufferContainerLocator.Get();
            NativeArray<uint> phaseIndices = new(particleCount, Allocator.Temp);
            for (int i = 0; i < globalState.KernelValue; i++)
            {
                int count = 0;
                for (int j = 0; j < particleCount; j++)
                {
                    // phaseIndicesArrayの値は1始まりなので-1して比較。
                    if (phaseArray[j] - 1 == i)
                    {
                        phaseIndices[count] = (uint)j;
                        count++;
                    }
                }

                container.PhaseIndicesBuffers[i].SetData(phaseIndices);
                globalState.PhaseCountArray[i] = count;
            }

            // メモリの解放。
            phaseArray.Dispose();
        }

        private EntityQuery _phase1EntityQuery;
        private EntityQuery _phase2EntityQuery;
    }
    
    [BurstCompile]
    public partial struct InitializePhaseArrayJob : IJobEntity
    {
        public NativeArray<int> PhaseOutput;

        void Execute(in ParticleEntity particle)
        {
            PhaseOutput[particle.Index] = particle.Phase;
        }
    }

    [BurstCompile]
    public partial struct Phase1UpdateJob : IJobEntity
    {
        public float DeltaTime;
        public Phase2Configs Phase2Configs;
        public EntityCommandBuffer.ParallelWriter ECB;

        public NativeArray<int> PhaseOutput;

        void Execute(
            [EntityIndexInQuery] int entityIndex,
            Entity entity,
            ref ParticleEntity particle,
            ref Phase1TimerEntity timer)
        {
            timer.ElapsedTime += DeltaTime;
            int index = particle.Index;

            if (timer.Timer < timer.ElapsedTime)
            {
                particle.Phase = 2;
                ECB.RemoveComponent<Phase1TimerEntity>(entityIndex, entity);
                Phase2TimerEntity newTimer = new Phase2TimerEntity(Phase2Configs.Duration);
                ECB.AddComponent(entityIndex, entity, newTimer);
            }

            PhaseOutput[index] = particle.Phase;
        }
    }

    [BurstCompile]
    public partial struct Phase2UpdateJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;

        public NativeArray<int> PhaseOutput;

        void Execute(
            [EntityIndexInQuery] int entityIndex,
            Entity entity,
            ref ParticleEntity particle,
            ref Phase2TimerEntity timer)
        {
            int index = particle.Index;

            timer.ElapsedTime += DeltaTime;

            if (timer.Timer < timer.ElapsedTime)
            {
                particle.Phase = 3;
                ECB.RemoveComponent<Phase2TimerEntity>(entityIndex, entity);
            }

            PhaseOutput[index] = particle.Phase;
        }
    }

    // TODO:
    // フェーズごとのパーティクル量のカウントと
    // インデックス配列の生成を並列Job化する
    // 先にパーティクルカウントを計算し、
    // フェーズごとに並び替えたインデックスの配列を一つ作り
    // スライスしてSetDataする
}