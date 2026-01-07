using Master.Configs;
using System;
using Unity.Entities;

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

        public void CreateSystems(int count, int kernelValue,
            Phase1Configs phase1, Phase2Configs phase2)
        {
            GlobalState globalState = new(count, kernelValue, phase1, phase2);
            Entity globalStateEntity = _entityManager.CreateEntity(typeof(GlobalState));
            _entityManager.SetComponentData(globalStateEntity, globalState);
            _globalStateQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<GlobalState>());

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

        public GlobalState GetGlobalState() => _globalStateQuery.GetSingleton<GlobalState>();

        public void Dispose()
        {
            _entityManager.DestroyEntity(_globalStateQuery);

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

        private EntityQuery _globalStateQuery;
    }
}