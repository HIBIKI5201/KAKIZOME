using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Master.Configs
{
    [Serializable]
    public struct Phase2Configs
    {
        public float DurationToPhase3 => _durationToPhase3;
        public float DurationToPhaseFinal => _durationToPhaseFinal;
        public float TorusSpeed => _torusSpeed;
        public float TorusMaxRadius => _torusRadius.y;
        public float TorusMinRadius => _torusRadius.x;
        public float TorusYSpring => _torusYSpring;

        public static void OnDrawGizmos(Phase2Configs configs, Vector3 center)
        {
            const int VERTEX_COUNT = 360;

            Span<Vector3> inPos = stackalloc Vector3[VERTEX_COUNT];
            Span<Vector3> outPos = stackalloc Vector3[VERTEX_COUNT];

            for (int i = 0; i < VERTEX_COUNT; i++)
            {
                float t = (float)i / VERTEX_COUNT;
                float rad = t * Mathf.PI * 2f;

                float cos = Mathf.Cos(rad);
                float sin = Mathf.Sin(rad);

                inPos[i] = new Vector3(sin * configs.TorusMinRadius, 0, cos * configs.TorusMinRadius) + center;
                outPos[i] = new Vector3(sin * configs.TorusMaxRadius, 0, cos * configs.TorusMaxRadius) + center;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLineList(inPos);
            Gizmos.DrawLineList(outPos);
        }

        [SerializeField, Tooltip("フェーズ３までの時間")]
        private float _durationToPhase3;
        [SerializeField, Tooltip("フェーズファイナルまでの時間")]
        private float _durationToPhaseFinal;
        [SerializeField, Tooltip("円環の回転速度")]
        private float _torusSpeed;
        [SerializeField, Tooltip("円環の半径"), MinMaxRangeSlider(0, 30)]
        private Vector2 _torusRadius;
        [SerializeField, Tooltip("円環のYバネ力")]
        private float _torusYSpring;
    }
}