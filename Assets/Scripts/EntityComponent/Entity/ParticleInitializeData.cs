using Unity.Entities;

namespace Master.Entities
{
    /// <summary>
    ///     パーティクル初期化データ。
    /// </summary>
    public readonly struct ParticleInitializeData
    {
        public ParticleInitializeData(int count, float phase1Duration)
        {
            Count = count;
            Phase1Duration = phase1Duration;
        }

        public readonly int Count;
        public readonly float Phase1Duration;
    }
}
