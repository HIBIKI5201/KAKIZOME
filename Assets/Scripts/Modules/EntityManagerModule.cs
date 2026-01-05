using System;
using Unity.Entities;
using UnityEngine;

namespace Master.Entities
{
    public class EntityManagerModule : IDisposable
    {
        public EntityManagerModule(World world)
        {
            _world = world;
            _entityManager = world.EntityManager;
            _group = _world.CreateSystemManaged<ParticleSystemGroup>();
        }

        public void CreateSystems(int count, int kernelValue, Phase1Configs phase1)
        {
            GlobalState globalState = new(count, kernelValue);
            Entity globalStateEntity = _entityManager.CreateEntity(typeof(GlobalState));
            _entityManager.SetComponentData(globalStateEntity, globalState);
            _globalStateQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<GlobalState>());

            ParticleInitializeEntity particleInitializeEntity = new(count, phase1.Duration);
            Entity particleInitializeDataEntity = _entityManager.CreateEntity(typeof(ParticleInitializeEntity));
            _entityManager.SetComponentData(particleInitializeDataEntity, particleInitializeEntity);

            SystemHandle initializeSystem = _world.CreateSystem<ParticleInitializeSystem>();
            SystemHandle phaseSystem = _world.CreateSystem<PhaseUpdateSystem>();

            _group.AddSystemToUpdateList(initializeSystem);
            _group.AddSystemToUpdateList(phaseSystem);

            _group.SortSystems();
        }

        public void UpdateSystems()
        {
            _group.Update();
        }

        public GlobalState GetGlobalState() => _globalStateQuery.GetSingleton<GlobalState>();

        public void Dispose()
        {
            if (_globalStateQuery != null)
            {
                GlobalState globalState = _globalStateQuery.GetSingleton<GlobalState>();
                globalState.Dispose();
                _globalStateQuery.Dispose();
            }
        }

        [Serializable]
        public struct Phase1Configs
        {
            public float Duration => _duration;

            [SerializeField, Tooltip("フェーズ１の長さ")]
            private float _duration;
        }

        private readonly World _world;
        private readonly EntityManager _entityManager;
        private readonly ParticleSystemGroup _group;

        private EntityQuery _globalStateQuery;
    }
}