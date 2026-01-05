using System;
using UnityEngine;

namespace Master.Modules
{
    /// <summary>
    ///     コンピュートシェーダーにデータを転送するモジュール。
    /// </summary>
    public class ComputeShaderTransferModule
    {
        public ComputeShaderTransferModule(ComputeShaderData data)
        {
            ComputeShaderData.Assert(data);

            _data = data;
            _kernelIndex = _data.Shader.FindKernel(KERNEL_NAME);
        }

        public void BindParameter(
            IGraphicBufferContainer bufferContainer,
            int count,
            float speed,
            float stopDistance)
        {
            _data.Shader.SetBuffer(_kernelIndex, _data.PositionBufferName, bufferContainer.PositionBuffer);
            _data.Shader.SetBuffer(_kernelIndex, _data.TargetBufferName, bufferContainer.TargetBuffer);
            _data.Shader.SetBuffer(_kernelIndex, _data.PhaseBufferName, bufferContainer.PhaseBuffer);
            _data.Shader.SetInt(_data.ParticleCountName, count);
            _data.Shader.SetFloat(_data.SpeedName, speed);
            _data.Shader.SetFloat(_data.StopDistanceName, stopDistance);
            _agentCount = count;
        }

        public void Dispatch(float deltaTime)
        {
            _data.Shader.SetFloat(_data.DeltaTimeName, deltaTime);

            int threadGroups = Mathf.CeilToInt(_agentCount / 256f);
            _data.Shader.Dispatch(_kernelIndex, threadGroups, 1, 1);
        }

        private const string KERNEL_NAME = "CSMain";

        private readonly int _kernelIndex;
        private readonly ComputeShaderData _data;

        private int _agentCount;

        [Serializable]
        public struct ComputeShaderData
        {
            public ComputeShader Shader => _shader;
            public string PositionBufferName => _positionBufferName;
            public string TargetBufferName => _targetBufferName;
            public string PhaseBufferName => _phaseBufferName;
            public string ParticleCountName => _particleCountName;
            public string DeltaTimeName => _deltaTimeName;
            public string SpeedName => _speedName;
            public string StopDistanceName => _stopDistanceName;

            public static void Assert(ComputeShaderData data)
            {
                Debug.Assert(data.Shader != null, "ComputeShader is null");
                Debug.Assert(!string.IsNullOrEmpty(data.PositionBufferName), "PositionBufferName is null");
                Debug.Assert(!string.IsNullOrEmpty(data.TargetBufferName), "TargetBufferName is null");
                Debug.Assert(!string.IsNullOrEmpty(data.PhaseBufferName), "PhaseBufferName is null");
                Debug.Assert(!string.IsNullOrEmpty(data.ParticleCountName), "ParticleCountName is null");
                Debug.Assert(!string.IsNullOrEmpty(data.DeltaTimeName), "DeltaTimeName is null");            
                Debug.Assert(!string.IsNullOrEmpty(data.SpeedName), "SpeedName is null");
                Debug.Assert(!string.IsNullOrEmpty(data.StopDistanceName), "StopDistanceName is null");
            }

            [SerializeField, Tooltip("シェーダー")]
            private ComputeShader _shader;

            [Space]
            [SerializeField, Tooltip("位置バッファ名")]
            private string _positionBufferName;
            [SerializeField, Tooltip("目標バッファ名")]
            private string _targetBufferName;
            [SerializeField, Tooltip("フェーズバッファ名")]
            private string _phaseBufferName;
            [SerializeField, Tooltip("パーティクル数パラメータ名")]
            private string _particleCountName;
            [SerializeField, Tooltip("デルタタイム名")]
            private string _deltaTimeName;
            [SerializeField, Tooltip("速度パラメータ名")]
            private string _speedName;
            [SerializeField, Tooltip("停止距離パラメータ名")]
            private string _stopDistanceName;
        }
    }
}