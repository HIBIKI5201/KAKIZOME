using UnityEngine;

namespace Master.Configs
{
    public interface IParticleCountContainer
    {
        public static IParticleCountContainer Instance { get; protected set; }

        public int ParticleCount { get; }
    }
}
