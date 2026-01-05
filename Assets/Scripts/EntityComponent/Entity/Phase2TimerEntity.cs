using Unity.Entities;
using UnityEngine;

namespace Master.Entities
{
    public struct Phase2TimerEntity : IComponentData
    {
        public Phase2TimerEntity(float timer)
        {
            Timer = timer;
            ElapsedTime = 0f;
        }

        public readonly float Timer;
        public float ElapsedTime;
    }
}