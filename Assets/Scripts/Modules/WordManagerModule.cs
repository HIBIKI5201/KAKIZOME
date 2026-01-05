using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Master.Modules
{
    /// <summary>
    ///     ワールド内の単語データを管理するモジュール。
    /// </summary>
    public class WordManagerModule
    {
        public WordManagerModule(WordData[] words)
        {
            _words = words;
        }

        /// <summary>
        ///     複数のメッシュの表面から、面積に応じて均一にサンプル位置を取得します。
        /// </summary>
        /// <param name="output">サンプル位置の出力配列</param>
        public void GetSampledPositions(ref NativeArray<float3> output)
        {
            int sampleCount = output.Length;
            using NativeList<Triangle> allTriangles = new(Allocator.Temp);
            float totalArea = 0f;

            #region ワードごとのデータを取得
            foreach (var word in _words)
            {
                if (word.Mesh == null) continue;

                Vector3 position = word.Position;
                Quaternion rotation = word.Rotation;
                float scale = word.Scale;
                Mesh mesh = word.Mesh;
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    // ローカル座標系の頂点をワールド座標に変換。
                    Vector3 v1Local = vertices[triangles[i]] * scale;
                    Vector3 v2Local = vertices[triangles[i + 1]] * scale;
                    Vector3 v3Local = vertices[triangles[i + 2]] * scale;

                    Vector3 v1 = position + rotation * v1Local;
                    Vector3 v2 = position + rotation * v2Local;
                    Vector3 v3 = position + rotation * v3Local;

                    Triangle tri = new(v1, v2, v3);

                    allTriangles.Add(tri);
                    totalArea += tri.Area;
                }
            }
            #endregion

            // バリデーションチェック。
            if (allTriangles.Length == 0)
            {
                Debug.LogWarning("三角ポリゴンがありません。");
                return;
            }

            #region ジョブでサンプルを計算
            // 累積面積配列を作る。
            NativeArray<float> cumulativeArray = new(allTriangles.Length, Allocator.TempJob);
            float runningTotal = 0f;
            for (int i = 0; i < allTriangles.Length; i++)
            {
                runningTotal += allTriangles[i].Area;
                cumulativeArray[i] = runningTotal / totalArea;
            }

            // ジョブ用の三角配列を作る。
            NativeArray<SampleTriangleJob.TriangleForJob> trianglesArray = new(allTriangles.Length, Allocator.TempJob);
            for (int i = 0; i < allTriangles.Length; i++)
            {
                trianglesArray[i] = new(allTriangles[i]);
            }

            // ジョブを実行。
            var job = new SampleTriangleJob
            {
                Triangles = trianglesArray,
                CumulativeAreas = cumulativeArray,
                Output = output,
                Seed = (uint)UnityEngine.Random.Range(1, int.MaxValue)
            };
            JobHandle handle = job.Schedule(sampleCount, 64);
            handle.Complete();

            #endregion

            trianglesArray.Dispose();
            cumulativeArray.Dispose();
        }

        private readonly WordData[] _words;

        /// <summary>
        ///     ポリゴン情報。
        /// </summary>
        private readonly struct Triangle
        {
            // 各頂点位置。
            public readonly Vector3 V1 => _v1;
            public readonly Vector3 V2 => _v2;
            public readonly Vector3 V3 => _v3;

            /// <summary> 三角形の面積 </summary>
            public readonly float Area => _area;

            private readonly Vector3 _v1, _v2, _v3;
            private readonly float _area;

            public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
            {
                _v1 = v1;
                _v2 = v2;
                _v3 = v3;

                // 外積の大きさの半分が三角形の面積。
                _area = math.length(math.cross(v2 - v1, v3 - v1)) * 0.5f;
            }
        }

        /// <summary>
        ///     三角ポリゴンのランダムな表面位置を取得する。
        /// </summary>
        [BurstCompile]
        private struct SampleTriangleJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<TriangleForJob> Triangles;
            [ReadOnly] public NativeArray<float> CumulativeAreas;

            public uint Seed;

            [WriteOnly] public NativeArray<float3> Output;

            public readonly struct TriangleForJob
            {
                public TriangleForJob(Triangle tri)
                {
                    _v1 = tri.V1;
                    _v2 = tri.V2;
                    _v3 = tri.V3;
                }

                public float3 V1 => _v1;
                public float3 V2 => _v2;
                public float3 V3 => _v3;

                private readonly float3 _v1, _v2, _v3;
            }

            public void Execute(int index)
            {
                Random rand = new(Seed + (uint)index);

                // 面積割合に基づいてどの三角形かを選択。
                float sample = rand.NextFloat();
                int triIndex = BinarySearch(CumulativeAreas, sample);

                TriangleForJob tri = Triangles[triIndex];

                // 三角形内の均一なランダム点を生成（バリセントリック座標）
                float r1 = math.sqrt(rand.NextFloat());
                float r2 = rand.NextFloat();

                float u = 1f - r1;
                float v = r1 * (1f - r2);
                float w = r1 * r2;

                Output[index] = u * tri.V1 + v * tri.V2 + w * tri.V3;
            }

            /// <summary>
            ///     二分探索で入力のインデックスを探す。
            /// </summary>
            /// <param name="array">ソート済み配列</param>
            /// <param name="value">検索値</param>
            /// <returns>インデックス</returns>
            private static int BinarySearch(NativeArray<float> array, float value)
            {
                int low = 0;
                int high = array.Length - 1;

                while (low <= high)
                {
                    int mid = (low + high) >> 1;
                    float midValue = array[mid];

                    if (midValue < value)
                    {
                        low = mid + 1;
                    }
                    else if (value < midValue)
                    {
                        high = mid - 1;
                    }
                    else
                    {
                        return mid;
                    }
                }

                // 見つからない場合は次の要素。
                return math.min(low, array.Length - 1);
            }
        }

        /// <summary>
        ///     文字データ。
        /// </summary>
        [Serializable]
        public class WordData
        {
            public Mesh Mesh => _mesh;
            public Vector3 Position => _position;
            public Quaternion Rotation => _rotation;
            public float Scale => _scale;

            [SerializeField, Tooltip("メッシュ")]
            private Mesh _mesh = null;
            [SerializeField, Tooltip("位置")]
            private Vector3 _position = Vector3.zero;
            [SerializeField, Tooltip("回転")]
            private Quaternion _rotation = Quaternion.identity;
            [SerializeField, Tooltip("スケール"), Min(0.1f)]
            private float _scale = 1;
        }

        /// <summary>
        ///     文字のギズモを表示。
        /// </summary>
        /// <param name="words"></param>
        public static void OnDrawGizmos(WordData[] words)
        {
            // 単語のメッシュをワイヤーフレームで描画。
            Gizmos.color = Color.green;
            foreach (var word in words)
            {
                if (word.Mesh != null)
                {
                    Gizmos.DrawWireMesh(word.Mesh, word.Position, word.Rotation, Vector3.one * word.Scale);
                }
            }
        }
    }
}