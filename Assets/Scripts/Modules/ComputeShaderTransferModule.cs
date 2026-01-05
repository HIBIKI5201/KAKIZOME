using System;
using System.Linq;
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
            PhaseKernelData[] kernelDatas = data.KernelDastas;
            _kernelIndexs = new int[kernelDatas.Length];
            for (int i = 0; i < kernelDatas.Length; i++)
            {
                _kernelIndexs[i] = data.Shader.FindKernel(kernelDatas[i].KernelName);
            }
        }

        public void BindParameter(
            IGraphicBufferContainer bufferContainer,
            int count,
            float speed,
            float stopDistance)
        {
            for (int i = 0; i < _kernelIndexs.Length; i++)
            {
                int index = _kernelIndexs[i];
                _data.Shader.SetBuffer(index, _data.PositionBufferName, bufferContainer.PositionBuffer);
                _data.Shader.SetBuffer(index, _data.TargetBufferName, bufferContainer.TargetBuffer);

                _data.Shader.SetBuffer(index, _data.KernelDastas[i].IndexBufferName, bufferContainer.PhaseIndicesBuffers[i]);
            }

            _data.Shader.SetInt(_data.ParticleCountName, count);
            _data.Shader.SetFloat(_data.SpeedName, speed);
            _data.Shader.SetFloat(_data.StopDistanceName, stopDistance);
        }

        public void Dispatch(float deltaTime, GraphicsBuffer[] phaseBuffers, ReadOnlySpan<int> counts)
        {
            _data.Shader.SetFloat(_data.DeltaTimeName, deltaTime);

            for (int i = 0; i < _kernelIndexs.Length; i++)
            {
                if (counts[i] == 0) { continue; }

                int threadGroups = Mathf.CeilToInt(counts[i] / 64f);

                _data.Shader.SetInt(_data.KernelDastas[i].CounterName, counts[i]);
                _data.Shader.Dispatch(_kernelIndexs[i], threadGroups, 1, 1);
            }
        }

        private readonly int[] _kernelIndexs;
        private readonly ComputeShaderData _data;

        [Serializable]
        public struct PhaseKernelData
        {
            public string KernelName => _kernelName;
            public string IndexBufferName => _indexBufferName;
            public string CounterName => _counterName;

            [SerializeField, Tooltip("カーネル名")]
            private string _kernelName;
            [SerializeField, Tooltip("インデックスバッファ名")]
            private string _indexBufferName;
            [SerializeField, Tooltip("カウンター名")]
            private string _counterName;
        }

        [Serializable]
        public struct ComputeShaderData
        {
            public ComputeShader Shader => _shader;
            public PhaseKernelData[] KernelDastas => _kernelNames;
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
            private PhaseKernelData[] _kernelNames;

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