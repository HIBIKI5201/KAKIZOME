using System;
using Unity.Entities.UniversalDelegates;
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
            PhaseKernelData[] kernelDatas = data.PhaseKernelDastas;
            _phaseKernelIndexs = new int[kernelDatas.Length];
            for (int i = 0; i < kernelDatas.Length; i++)
            {
                _phaseKernelIndexs[i] = data.Shader.FindKernel(kernelDatas[i].KernelName);
            }
            _colorKernelIndex = data.Shader.FindKernel(data.ColorKernelName);
        }

        public void BindParameter(
            IGraphicBufferContainer bufferContainer,
            int count,
            Vector3 centerPos, float radius,
            float speed,
            float stopDistance)
        {
            for (int i = 0; i < _phaseKernelIndexs.Length; i++)
            {
                int index = _phaseKernelIndexs[i];
                _data.Shader.SetBuffer(index, _data.PositionBufferName, bufferContainer.PositionBuffer);
                _data.Shader.SetBuffer(index, _data.VelocityBufferName, bufferContainer.VelocityBuffer);
                _data.Shader.SetBuffer(index, _data.TargetBufferName, bufferContainer.TargetBuffer);

                _data.Shader.SetBuffer(index, _data.PhaseKernelDastas[i].IndexBufferName, bufferContainer.PhaseIndicesBuffers[i]);
            }

            _data.Shader.SetBuffer(_colorKernelIndex, _data.PositionBufferName, bufferContainer.PositionBuffer);
            _data.Shader.SetBuffer(_colorKernelIndex, _data.ColorBufferName, bufferContainer.ColorBuffer);

            _data.Shader.SetInt(_data.ParticleCountName, count);
            _data.Shader.SetVector(_data.CenterPositionName, centerPos);
            _data.Shader.SetFloat(_data.RotationRadiusName, radius);
            _data.Shader.SetFloat(_data.SpeedName, speed);
            _data.Shader.SetFloat(_data.StopDistanceName, stopDistance);
        }

        public void Dispatch(float deltaTime, float time,
            GraphicsBuffer[] phaseBuffers, ReadOnlySpan<int> counts,
            int count)
        {
            _data.Shader.SetFloat(_data.DeltaTimeName, deltaTime);
            _data.Shader.SetFloat(_data.TimeName, time);

            for (int i = 0; i < _phaseKernelIndexs.Length; i++)
            {
                if (counts[i] == 0) { continue; }

                int threadGroups = Mathf.CeilToInt(counts[i] / 64f);

                _data.Shader.SetInt(_data.PhaseKernelDastas[i].CounterName, counts[i]);
                _data.Shader.Dispatch(_phaseKernelIndexs[i], threadGroups, 1, 1);
            }

            int tg = Mathf.CeilToInt(count / 256f);
            _data.Shader.Dispatch(_colorKernelIndex, tg, 1, 1);
        }

        private readonly int[] _phaseKernelIndexs;
        private readonly int _colorKernelIndex;
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
            public PhaseKernelData[] PhaseKernelDastas => _phaseKernelNames;
            public string ColorKernelName => _colorKernelName;
            public string PositionBufferName => _positionBufferName;
            public string VelocityBufferName => _velocityBufferName;
            public string TargetBufferName => _targetBufferName;
            public string PhaseBufferName => _phaseBufferName;
            public string ColorBufferName => _colorBufferName;
            public string ParticleCountName => _particleCountName;
            public string CenterPositionName => _centerPositionName;
            public string RotationRadiusName => _rotationRadiusName;
            public string TimeName => _timeName;
            public string DeltaTimeName => _deltaTimeName;
            public string SpeedName => _speedName;
            public string StopDistanceName => _stopDistanceName;

            public static void Assert(ComputeShaderData data)
            {
                Debug.Assert(data.Shader != null, "ComputeShader is null");
                Debug.Assert(data.PhaseKernelDastas.Length != 0, $"{nameof(PhaseKernelDastas)} length is 0");
                Debug.Assert(!string.IsNullOrEmpty(data.ColorKernelName), $"{nameof(PositionBufferName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.PositionBufferName), $"{nameof(PositionBufferName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.VelocityBufferName), $"{nameof(VelocityBufferName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.TargetBufferName), $"{nameof(TargetBufferName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.PhaseBufferName), $"{nameof(PhaseBufferName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.ColorBufferName), $"{nameof(PhaseBufferName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.ParticleCountName), $"{nameof(ParticleCountName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.CenterPositionName), $"{nameof(CenterPositionName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.RotationRadiusName), $"{nameof(RotationRadiusName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.TimeName), $"{nameof(TimeName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.DeltaTimeName), $"{nameof(DeltaTimeName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.SpeedName), $"{nameof(SpeedName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.StopDistanceName), $"{nameof(StopDistanceName)} is null");
            }

            [SerializeField, Tooltip("シェーダー")]
            private ComputeShader _shader;

            [Space]
            [SerializeField, Tooltip("フェーズのカーネル名")]
            private PhaseKernelData[] _phaseKernelNames;
            [SerializeField, Tooltip("")]
            private string _colorKernelName;

            [Space]
            [SerializeField, Tooltip("位置バッファ名")]
            private string _positionBufferName;
            [SerializeField, Tooltip("速度バッファ名")]
            private string _velocityBufferName;
            [SerializeField, Tooltip("目標バッファ名")]
            private string _targetBufferName;
            [SerializeField, Tooltip("フェーズバッファ名")]
            private string _phaseBufferName;
            [SerializeField, Tooltip("色バッファ名")]
            private string _colorBufferName;
            [SerializeField, Tooltip("パーティクル数パラメータ名")]
            private string _particleCountName;
            [SerializeField, Tooltip("中心位置パラメータ名")]
            private string _centerPositionName;
            [SerializeField, Tooltip("回転半径")]
            private string _rotationRadiusName;
            [SerializeField, Tooltip("タイム名")]
            private string _timeName;
            [SerializeField, Tooltip("デルタタイム名")]
            private string _deltaTimeName;
            [SerializeField, Tooltip("速度パラメータ名")]
            private string _speedName;
            [SerializeField, Tooltip("停止距離パラメータ名")]
            private string _stopDistanceName;
        }
    }
}