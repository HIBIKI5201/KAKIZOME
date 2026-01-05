using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Master.Modules
{
    /// <summary>
    ///     GPUバッファを管理するモジュール。
    /// </summary>
    public class GPUBufferContainerModule : IDisposable, IGraphicBufferContainer
    {
        public GPUBufferContainerModule(int count)
        {
            _positionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, sizeof(float) * 3);
            _targetBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, sizeof(float) * 3);
            _phaseBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, sizeof(int));
            _count = count;
        }

        public GraphicsBuffer PositionBuffer => _positionBuffer;
        public GraphicsBuffer TargetBuffer => _targetBuffer;
        public GraphicsBuffer PhaseBuffer => _phaseBuffer;

        public void InitializeData(WordManagerModule word, InitialSphereModule sphere)
        {
            // 目標位置をワードメッシュ上にサンプリング。
            NativeArray<float3> targetPos = new(_count, Allocator.TempJob);
            word.GetSampledPositions(ref targetPos);
            _targetBuffer.SetData(targetPos);
            targetPos.Dispose();

            // 初期位置を球面上にランダム配置。
            NativeArray<float3> initialPos = new NativeArray<float3>(_count, Allocator.Temp);
            sphere.RandomArray(ref initialPos);
            _positionBuffer.SetData(initialPos);
            initialPos.Dispose();

            // 全てのフェーズを0に初期化。
            NativeArray<int> phaseData = new NativeArray<int>(_count, Allocator.Temp);
            for (int i = 0; i < _count; i++) { phaseData[i] = 0; }
            _phaseBuffer.SetData(phaseData);
            phaseData.Dispose();
        }

        public void Dispose()
        {
            _positionBuffer.Dispose();
            _targetBuffer.Dispose();
            _phaseBuffer.Dispose();
        }

        private readonly GraphicsBuffer _positionBuffer;
        private readonly GraphicsBuffer _targetBuffer;
        private readonly GraphicsBuffer _phaseBuffer;

        private readonly int _count;
    }
}
