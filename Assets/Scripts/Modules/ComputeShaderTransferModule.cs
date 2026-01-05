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

            // カーネルインデックス取得。
            string[] kernelNames = data.KernelNames;
            _kernelIndexs = new int[kernelNames.Length];
            for (int i = 0; i < kernelNames.Length; i++)
            {
                _kernelIndexs[i] = data.Shader.FindKernel(kernelNames[i]);
            }
        }

        public void BindParameter(
            IGraphicBufferContainer bufferContainer,
            int count,
            float speed,
            float stopDistance)
        {
            foreach (var index in _kernelIndexs)
            {
                _data.Shader.SetBuffer(index, _data.PositionBufferName, bufferContainer.PositionBuffer);
                _data.Shader.SetBuffer(index, _data.TargetBufferName, bufferContainer.TargetBuffer);
                _data.Shader.SetBuffer(index, _data.PhaseBufferName, bufferContainer.PhaseBuffer);
            }

            _data.Shader.SetInt(_data.ParticleCountName, count);
            _data.Shader.SetFloat(_data.SpeedName, speed);
            _data.Shader.SetFloat(_data.StopDistanceName, stopDistance);
            _agentCount = count;
        }

        public void Dispatch(float deltaTime, GraphicsBuffer[] phaseBuffers)
        {
            _data.Shader.SetFloat(_data.DeltaTimeName, deltaTime);

            int threadGroups = Mathf.CeilToInt(_agentCount / 256f);
            _data.Shader.Dispatch(_kernelIndexs[0], threadGroups, 1, 1);
        }

        private readonly int[] _kernelIndexs;
        private readonly ComputeShaderData _data;

        private int _agentCount;

        [Serializable]
        public struct ComputeShaderData
        {
            public ComputeShader Shader => _shader;
            public string[] KernelNames => _kernelNames;
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
            [SerializeField, Tooltip("カーネル名")]
            private string[] _kernelNames;

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