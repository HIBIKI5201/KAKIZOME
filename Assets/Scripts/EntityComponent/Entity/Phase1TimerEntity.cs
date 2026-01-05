using Unity.Entities;

namespace Master.Entities
{
    /// <summary>
    /// フェーズ1のタイマーを表すエンティティコンポーネント。
    /// フェーズ1に関連する時間管理に使用される。
    /// </summary>
    public struct Phase1TimerEntity : IComponentData
    {
        public Phase1TimerEntity(float timer)
        {
            Timer = timer;
            ElapsedTime = 0f;
        }

        public readonly float Timer;
        public float ElapsedTime;
    }
}