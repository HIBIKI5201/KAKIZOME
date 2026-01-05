using Unity.Entities;

namespace Master.Entities
{
    /// <summary>
    ///     パーティクルエンティティを示すマーカーコンポーネント。
    /// </summary>
    public struct ParticleEntity : IComponentData
    {
        public int Phase;
    }
}