using System;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

namespace Master.Configs
{
    [Serializable]
    public struct Phase3Configs
    {
        public uint OrbitalSphereCount => _orbitalSphereCount;
        public float OrbitalSphereRadius => _orbitalSphereRadius;
        public Vector3 OrbitalCenterOffset => _orbitalCenterOffset;
        public float OrbitalSpeed => _orbitalSpeed;
        public float OrbitalMaxRadius => _orbitalRadius.y;
        public float OrbitalMinRadius => _orbitalRadius.x;
        public float PosGain => _posGain;
        public float VelGain => _velGain;

        public static void OnDrawGizmos(Phase3Configs configs, Vector3 center)
        {
            const int VERTEX_COUNT = 360;

            center += configs.OrbitalCenterOffset;

            Span<Vector3> inPos = stackalloc Vector3[VERTEX_COUNT];
            Span<Vector3> outPos = stackalloc Vector3[VERTEX_COUNT];

            for (int i = 0; i < VERTEX_COUNT; i++)
            {
                float t = (float)i / VERTEX_COUNT;
                float rad = t * Mathf.PI * 2f;

                float cos = Mathf.Cos(rad);
                float sin = Mathf.Sin(rad);

                inPos[i] = new Vector3(sin * configs.OrbitalMinRadius, 0, cos * configs.OrbitalMinRadius) + center;
                outPos[i] = new Vector3(sin * configs.OrbitalMaxRadius, 0, cos * configs.OrbitalMaxRadius) + center;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawLineList(inPos);
            Gizmos.DrawLineList(outPos);
        }

        [SerializeField, Tooltip("衛星球の数")]
        private uint _orbitalSphereCount;
        [SerializeField, Tooltip("衛星球の半径")]
        private float _orbitalSphereRadius;
        [SerializeField, Tooltip("衛星球の中心軸調整")]
        private Vector3 _orbitalCenterOffset;
        [SerializeField, Tooltip("衛星球の公転速度")]
        private float _orbitalSpeed;
        [SerializeField, Tooltip("衛星球の公転半径"), MinMaxRangeSlider(0, 30)]
        private Vector2 _orbitalRadius;
        [SerializeField, Tooltip("追従PD制御の位置ゲイン")]
        private float _posGain;
        [SerializeField, Tooltip("追従PD制御の速度ゲイン")]
        private float _velGain;
    }
}
