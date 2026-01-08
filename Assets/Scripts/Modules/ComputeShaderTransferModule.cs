using Master.Configs;
using System;
using Unity.Mathematics;
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

        public int KernelLength => _phaseKernelIndexs.Length;

        public void BindParameter(
            IGraphicBufferContainer bufferContainer,
            int count,
            PhaseConfigRepository phaseConfig)
        {
            ComputeShader shader = _data.Shader;

            #region フェーズカーネルのバインド
            for (int i = 0; i < _phaseKernelIndexs.Length; i++)
            {
                int index = _phaseKernelIndexs[i];
                shader.SetBuffer(index, _data.PositionBufferName, bufferContainer.PositionBuffer);
                shader.SetBuffer(index, _data.VelocityBufferName, bufferContainer.VelocityBuffer);
                shader.SetBuffer(index, _data.TargetBufferName, bufferContainer.TargetBuffer);

                shader.SetBuffer(index, _data.PhaseKernelDastas[i].IndexBufferName, bufferContainer.PhaseIndicesBuffers[i]);
            }
            #endregion

            #region 色カーネルのバインド
            shader.SetBuffer(_colorKernelIndex, _data.PositionBufferName, bufferContainer.PositionBuffer);
            shader.SetBuffer(_colorKernelIndex, _data.ColorBufferName, bufferContainer.ColorBuffer);
            #endregion

            #region パラメータ類のバインド
            // グローバル。
            shader.SetInt(_data.ParticleCountName, count);
            shader.SetVector(_data.CenterPositionName, phaseConfig.GlobalConfigs.CenterPosition);

            // フェーズ1。
            shader.SetFloat(_data.Phase1AccelName, phaseConfig.Phase1Configs.Accel);

            // フェーズ2。
            shader.SetFloat(_data.TorusMaxRadiusName, phaseConfig.Phase2Configs.TorusMaxRadius);
            shader.SetFloat(_data.TorusMinRadiusName, phaseConfig.Phase2Configs.TorusMinRadius);
            shader.SetFloat(_data.TorusSpeedName, phaseConfig.Phase2Configs.TorusSpeed);
            shader.SetFloat(_data.TorusYSpringName, phaseConfig.Phase2Configs.TorusYSpring);
            shader.SetFloat(_data.TorusTangentGainName, phaseConfig.Phase2Configs.TangentGain);
            shader.SetFloat(_data.TorusRadiusGainName, phaseConfig.Phase2Configs.RadiusGain);

            // フェーズ3。
            shader.SetFloat(_data.OrbitalSphereCountName, phaseConfig.Phase3Configs.OrbitalSphereCount);
            shader.SetFloat(_data.OrbitalSphereRadiusName, phaseConfig.Phase3Configs.OrbitalSphereRadius);
            shader.SetVector(_data.OrbitalCenterOffsetName, phaseConfig.Phase3Configs.OrbitalCenterOffset);
            shader.SetFloat(_data.OrbitalSpeedName, phaseConfig.Phase3Configs.OrbitalSpeed);
            shader.SetFloat(_data.OrbitalMaxRadiusName, phaseConfig.Phase3Configs.OrbitalMaxRadius);
            shader.SetFloat(_data.OrbitalMinRadiusName, phaseConfig.Phase3Configs.OrbitalMinRadius);
            shader.SetFloat(_data.OrbitalPosGainName, phaseConfig.Phase3Configs.PosGain);
            shader.SetFloat(_data.OrbitalVelGainName, phaseConfig.Phase3Configs.VelGain);

            // フェーズファイナル。
            shader.SetFloat(_data.StopDistanceName, phaseConfig.PhaseFinalConfigs.StopDistance);
            shader.SetFloat(_data.PhaseFinalFollowGainName, phaseConfig.PhaseFinalConfigs.FollowGain);
            #endregion
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
            
            // グローバル。
            public string ParticleCountName => _particleCountName;
            public string CenterPositionName => _centerPositionName;
            public string TimeName => _timeName;
            public string DeltaTimeName => _deltaTimeName;

            // フェーズ1。
            public string Phase1AccelName => _phase1AccelName;

            // フェーズ2。
            public string TorusMaxRadiusName => _torusMaxRadiusName;
            public string TorusMinRadiusName => _torusMinRadiusName;
            public string TorusSpeedName => _torusSpeedName;
            public string TorusYSpringName => _torusYSpringName;
            public string TorusTangentGainName => _torusTangentGainName;
            public string TorusRadiusGainName => _torusRadiusGainName;

            // フェーズ3。
            public string OrbitalSphereCountName => _orbitalSphereCountName;
            public string OrbitalSphereRadiusName => _orbitalSphereRadiusName;
            public string OrbitalCenterOffsetName => _orbitalCenterOffsetName;
            public string OrbitalSpeedName => _orbitalSpeedName;
            public string OrbitalMaxRadiusName => _orbitalMaxRadiusName;
            public string OrbitalMinRadiusName => _orbitalMinRadiusName;
            public string OrbitalPosGainName => _orbitalPosGainName;
            public string OrbitalVelGainName => _orbitalVelGainName;

            // フェーズファイナル。
            public string StopDistanceName => _stopDistanceName;
            public string PhaseFinalFollowGainName => _phaseFinalFollowGainName;

            public static void Assert(ComputeShaderData data)
            {
                Debug.Assert(data.Shader != null, "ComputeShader is null");
                Debug.Assert(data.PhaseKernelDastas.Length != 0, $"{nameof(PhaseKernelDastas)} length is 0");
                Debug.Assert(!string.IsNullOrEmpty(data.ColorKernelName), $"{nameof(ColorKernelName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.PositionBufferName), $"{nameof(PositionBufferName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.VelocityBufferName), $"{nameof(VelocityBufferName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.TargetBufferName), $"{nameof(TargetBufferName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.PhaseBufferName), $"{nameof(PhaseBufferName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.ColorBufferName), $"{nameof(ColorBufferName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.ParticleCountName), $"{nameof(ParticleCountName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.CenterPositionName), $"{nameof(CenterPositionName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.TimeName), $"{nameof(TimeName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.DeltaTimeName), $"{nameof(DeltaTimeName)} is null");

                Debug.Assert(!string.IsNullOrEmpty(data.Phase1AccelName), $"{nameof(Phase1AccelName)} is null");

                Debug.Assert(!string.IsNullOrEmpty(data.TorusMaxRadiusName), $"{nameof(TorusMaxRadiusName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.TorusMinRadiusName), $"{nameof(TorusMinRadiusName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.TorusSpeedName), $"{nameof(TorusSpeedName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.TorusYSpringName), $"{nameof(TorusYSpringName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.TorusTangentGainName), $"{nameof(TorusTangentGainName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.TorusRadiusGainName), $"{nameof(TorusRadiusGainName)} is null");
                
                Debug.Assert(!string.IsNullOrEmpty(data.OrbitalSphereCountName), $"{nameof(OrbitalSphereCountName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.OrbitalSphereRadiusName), $"{nameof(OrbitalSphereRadiusName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.OrbitalCenterOffsetName), $"{nameof(OrbitalCenterOffsetName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.OrbitalSpeedName), $"{nameof(OrbitalSpeedName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.OrbitalMaxRadiusName), $"{nameof(OrbitalMaxRadiusName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.OrbitalMinRadiusName), $"{nameof(OrbitalMinRadiusName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.OrbitalPosGainName), $"{nameof(OrbitalPosGainName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.OrbitalVelGainName), $"{nameof(OrbitalVelGainName)} is null");

                Debug.Assert(!string.IsNullOrEmpty(data.StopDistanceName), $"{nameof(StopDistanceName)} is null");
                Debug.Assert(!string.IsNullOrEmpty(data.PhaseFinalFollowGainName), $"{nameof(PhaseFinalFollowGainName)} is null");
            }

            [SerializeField, Tooltip("シェーダー")]
            private ComputeShader _shader;

            [Header("カーネル")]
            [SerializeField, Tooltip("フェーズのカーネル名")]
            private PhaseKernelData[] _phaseKernelNames;
            [SerializeField, Tooltip("")]
            private string _colorKernelName;

            [Header("バッファ")]
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

            [Header("グローバル")]
            [SerializeField, Tooltip("パーティクル数パラメータ名")]
            private string _particleCountName;
            [SerializeField, Tooltip("中心位置パラメータ名")]
            private string _centerPositionName;
            [SerializeField, Tooltip("タイム名")]
            private string _timeName;
            [SerializeField, Tooltip("デルタタイム名")]
            private string _deltaTimeName;

            [Header("フェーズ1")]
            [SerializeField, Tooltip("放射拡散の加速度")]
            private string _phase1AccelName;

            [Header("フェーズ2")]
            [SerializeField, Tooltip("円環最大半径パラメータ名")]
            private string _torusMaxRadiusName;
            [SerializeField, Tooltip("円環最小半径パラメータ名")]
            private string _torusMinRadiusName;
            [SerializeField, Tooltip("円環の回転速度パラメータ名")]
            private string _torusSpeedName;
            [SerializeField, Tooltip("円環のYバネ力パラメータ名")]
            private string _torusYSpringName;
            [SerializeField, Tooltip("接線方向の追従ゲイン")]
            private string _torusTangentGainName;
            [SerializeField, Tooltip("半径方向の拘束ゲイン")]
            private string _torusRadiusGainName;


            [Header("フェーズ3")]
            [SerializeField, Tooltip("衛星球の数パラメータ名")]
            private string _orbitalSphereCountName;
            [SerializeField, Tooltip("衛星球の半径パラメータ名")]
            private string _orbitalSphereRadiusName;
            [SerializeField, Tooltip("衛星球の中心軸調整パラメータ名")]
            private string _orbitalCenterOffsetName;
            [SerializeField, Tooltip("衛星球の公転速度パラメータ名")]
            private string _orbitalSpeedName;
            [SerializeField, Tooltip("衛星球の公転最大半径パラメータ名")]
            private string _orbitalMaxRadiusName;
            [SerializeField, Tooltip("衛星球の公転最小半径パラメータ名")]
            private string _orbitalMinRadiusName;
            [SerializeField, Tooltip("追従PD制御の位置ゲイン")]
            private string _orbitalPosGainName;
            [SerializeField, Tooltip("追従PD制御の速度ゲイン")]
            private string _orbitalVelGainName;

            [Header("フェーズファイナル")]
            [SerializeField, Tooltip("停止距離パラメータ名")]
            private string _stopDistanceName;
            [SerializeField, Tooltip("文字への追従ゲイン")]
            private string _phaseFinalFollowGainName;
        }
    }
}