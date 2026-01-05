using Unity.Entities;

namespace Master.Entities
{
    public struct GlobalState : IComponentData
    {
        public GlobalState(int count, float phase1Duration)
        {
            Count = count;
            Phase1Duration = phase1Duration;
        }

        public readonly int Count;
        public readonly float Phase1Duration;
    }
}
