using Unity.Entities;

namespace Master.Entities
{
    public struct PhaseCountEntity : IBufferElementData
    {
        public PhaseCountEntity(int i) => Count = i;

        public int Count;
    }
}
