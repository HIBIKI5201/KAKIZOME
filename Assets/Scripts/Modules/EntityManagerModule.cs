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

        public void CreateSystems(int count, int kernelValue, Phase1Configs phase1)
        {
            GlobalState globalState = new(count, kernelValue, phase1);
            Entity globalStateEntity = _entityManager.CreateEntity(typeof(GlobalState));
            _entityManager.SetComponentData(globalStateEntity, globalState);
            _globalStateQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<GlobalState>());

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

        private readonly World _world;
        private readonly EntityManager _entityManager;
        private readonly ParticleSystemGroup _group;

        private EntityQuery _globalStateQuery;
    }
}