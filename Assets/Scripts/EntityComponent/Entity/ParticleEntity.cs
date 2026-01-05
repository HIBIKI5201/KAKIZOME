using Unity.Entities;

namespace Master.Entities
{
    /// <summary>
    ///     パーティクルエンティティを示すマーカーコンポーネント。
    /// </summary>
    public struct ParticleEntity : IComponentData
    {
        public ParticleEntity(int index)
        {
            Index = index;
            Phase = 1;
        }

        public readonly int Index;
        public int Phase;
    }
}