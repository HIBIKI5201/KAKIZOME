using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Master.Modules
{
    /// <summary>
    ///     初期球体モジュール。
    /// </summary>
    public class InitialSphereModule
    {
        public InitialSphereModule(float radius, Vector3 center)
        {
            _radius = radius;
            _center = center;
        }

        /// <summary>
        ///     初期球のランダムな位置。
        /// </summary>
        /// <param name="array"></param>
        public void RandomArray(ref NativeArray<float3> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = _center + UnityEngine.Random.onUnitSphere * _radius;
            }
        }

        /// <summary>
        ///     上に上昇する。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="delta"></param>
        public void UpArray(ref NativeArray<float3> array, float delta)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] += new float3(0, 1, 0) * delta;
            }
        }

        private readonly float _radius;
        private readonly Vector3 _center;

        /// <summary>
        ///     初期球と中心のギズモを表示。
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="center"></param>
        public static void OnDrawGizmos(float radius, Vector3 center)
        {
            // 初期球体をワイヤーフレームで描画。
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(center, radius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center, 0.1f);
        }
    }
}