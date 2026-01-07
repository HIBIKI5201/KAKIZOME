using Master.Configs;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Rendering;

namespace Master.Entities
{
    public class EntityManagerModule : IDisposable
    {
        public EntityManagerModule(World world)
        {
            _world = world;
            _entityManager = world.EntityManager;
            _group = _world.CreateSystemManaged<ParticleSystemGroup>();
            _phaseCountEntity = _entityManager.CreateEntity();
        }

        public void CreateSystems(int count, int kernelValue,
            PhaseConfigRepository phaseConfig)
        {
            GlobalState globalState =
                new(count, kernelValue, phaseConfig.Phase1Configs, phaseConfig.Phase2Configs);
            Entity globalStateEntity = _entityManager.CreateEntity(typeof(GlobalState));
            _entityManager.SetComponentData(globalStateEntity, globalState);
            _globalStateQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<GlobalState>());

            // フェーズカウントの受け取りバッファを生成。
            DynamicBuffer<PhaseCountEntity> buffer =
                _entityManager.AddBuffer<PhaseCountEntity>(_phaseCountEntity);
            buffer.ResizeUninitialized(kernelValue);
            NativeArray<PhaseCountEntity> array = buffer.AsNativeArray();
            for (int i = 0; i < kernelValue; i++) { array[i] = default; }

            SystemHandle initializeSystem = _world.CreateSystem<ParticleInitializeSystem>();
            SystemHandle phase1System = _world.CreateSystem<Phase1UpdateSystem>();
            SystemHandle phase2System = _world.CreateSystem<Phase2UpdateSystem>();
            SystemHandle syncSystem = _world.CreateSystem<ParticlePhaseSyncSystem>();

            _group.AddSystemToUpdateList(initializeSystem);
            _group.AddSystemToUpdateList(phase1System);
            _group.AddSystemToUpdateList(phase2System);
            _group.AddSystemToUpdateList(syncSystem);

            _group.SortSystems();
        }

        public void UpdateSystems()
        {
            _group.Update();
        }

        public void GetPhaseCount(Span<int> output)
        {
            if (!_entityManager.HasBuffer<PhaseCountEntity>(_phaseCountEntity)) { return; }

            DynamicBuffer<PhaseCountEntity> buffer =
                _entityManager.GetBuffer<PhaseCountEntity>(_phaseCountEntity);
            NativeArray<PhaseCountEntity> array = buffer.AsNativeArray();
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = array[i].Count;
            }
        }

        public void Dispose()
        {
            if (!_world.IsCreated) { return; }

            _entityManager.DestroyEntity(_globalStateQuery);
            _entityManager.DestroyEntity(_phaseCountEntity);

            if (_group.Enabled)
            {
                foreach (var system in _group.ManagedSystems)
                {
                    _world.DestroySystemManaged(system);
                }
                _world.DestroySystemManaged(_group);
            }
        }

        private readonly World _world;
        private readonly EntityManager _entityManager;
        private readonly ParticleSystemGroup _group;
        private readonly Entity _phaseCountEntity;

        private EntityQuery _globalStateQuery;
    }
}