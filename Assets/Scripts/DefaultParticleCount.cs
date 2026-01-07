using System;
using UnityEngine;

namespace Master.Configs
{
    [Serializable]
    public struct DefaultParticleCount
    {
#if UNITY_EDITOR
        public int Value => _editorCount;
#else
        public int Value => _buildCount;
#endif

#if UNITY_EDITOR
        [SerializeField, Tooltip("エディタ用のパーティクル数")]
        private int _editorCount;
#endif
        [SerializeField, Tooltip("ビルド用のパーティクル数")]
        private int _buildCount;
    }
}
