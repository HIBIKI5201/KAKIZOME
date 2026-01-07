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
        public GPUBufferContainerModule(int count, int kernelValue)
        {
            _positionBuffer = new(GraphicsBuffer.Target.Structured, count, sizeof(float) * 3);
            _velocityBuffer = new(GraphicsBuffer.Target.Structured, count, sizeof(float) * 3);
            _targetBuffer = new(GraphicsBuffer.Target.Structured, count, sizeof(float) * 3);
            _count = count;
            _phaseIndicesBuffers = new GraphicsBuffer[kernelValue];
            for (int i = 0; i < kernelValue; i++)
            {
                _phaseIndicesBuffers[i] = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, sizeof(uint));
            }
            _colorBuffer = new(GraphicsBuffer.Target.Structured, count, sizeof(float) * 3);
        }

        public GraphicsBuffer PositionBuffer => _positionBuffer;
        public GraphicsBuffer VelocityBuffer => _velocityBuffer;
        public GraphicsBuffer TargetBuffer => _targetBuffer;
        public GraphicsBuffer[] PhaseIndicesBuffers => _phaseIndicesBuffers;
        public GraphicsBuffer ColorBuffer => _colorBuffer;

        public void InitializeData(WordManagerModule word, InitialSphereModule sphere)
        {
            // 目標位置をワードメッシュ上にサンプリング。
            NativeArray<float3> targetPos = new(_count, Allocator.TempJob);
            word.GetSampledPositions(ref targetPos);
            _targetBuffer.SetData(targetPos);
            targetPos.Dispose();

            // 初期位置を球面上にランダム配置。
            NativeArray<float3> initialPos = new(_count, Allocator.Temp);
            sphere.RandomArray(ref initialPos);
            _positionBuffer.SetData(initialPos);
            initialPos.Dispose();

            // 速度を初期化。
            NativeArray<float3> velocity = new(_count, Allocator.Temp);
            _velocityBuffer.SetData(velocity);
            _velocityBuffer.Dispose();
        }

        public void Dispose()
        {
            _positionBuffer.Release();
            _positionBuffer.Dispose();

            _velocityBuffer.Release();
            _velocityBuffer.Dispose();

            _targetBuffer.Release();
            _targetBuffer.Dispose();
            foreach (var buffer in _phaseIndicesBuffers)
            {
                buffer.Release();
                buffer.Dispose();
            }

            _colorBuffer.Release();
            _colorBuffer.Dispose();
        }

        private readonly GraphicsBuffer _positionBuffer;
        private readonly GraphicsBuffer _velocityBuffer;
        private readonly GraphicsBuffer _targetBuffer;
        private readonly GraphicsBuffer[] _phaseIndicesBuffers;
        private readonly GraphicsBuffer _colorBuffer;

        private readonly int _count;
    }
}
