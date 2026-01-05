using Master.Modules;
using System;
using Unity.Entities;
using UnityEngine.LightTransport;

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

        public void CreateSystems(int count)
        {
            GlobalState globalState = new(count, 5);
            Entity globalStateEntity = _entityManager.CreateEntity(typeof(GlobalState));
            _entityManager.SetComponentData(globalStateEntity, globalState);

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

        public void Dispose()
        {
            _world.DestroySystemManaged(_group);
            using var query = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<GlobalState>()
            );
            GlobalState globalState = query.GetSingleton<GlobalState>();
            globalState.Dispose();
        }

        private readonly World _world;
        private readonly EntityManager _entityManager;
        private readonly ParticleSystemGroup _group;
    }
}