using Master.Entities;
using Master.Modules;
using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.VFX;

namespace Master.Runner
{
    [RequireComponent(typeof(VisualEffect))]
    public class Runner : MonoBehaviour
    {
        [Header("基本設定")]
        [SerializeField]
        private Vector3 _centerPosition = Vector3.zero;
        [SerializeField]
        private int _particleCount = 1000;

        [Header("パーティクル設定")]
        [SerializeField, Tooltip("パーティクルの速度")]
        private float _particleSpeed = 5f;
        [SerializeField, Tooltip("パーティクルが停止する目標地点までの距離")]
        private float _particleStopDistance = 0.1f;

        [Header("フェーズ設定")]
        [Space]
        [SerializeField]
        private float _initialRadius = 20f;
        [Space]
        [SerializeField]
        private EntityManagerModule.Phase1Configs _phase1Configs;

        [Header("モジュールパラメータ設定")]
        [SerializeField, Tooltip("VFXのパラメータ名")]
        private VisualEffectTransferModule.VFXParameterNames _vfxParameterNames;
        [SerializeField, Tooltip("コンピュートシェーダーのパラメータ")]
        private ComputeShaderTransferModule.ComputeShaderData _computeShaderData;
        [SerializeField, Tooltip("文字データ")]
        private WordManagerModule.WordData[] _wordDataArray;

        private InitialSphereModule _initialSphereModule;
        private GPUBufferContainerModule _gpuBufferContainer;
        private EntityManagerModule _entityManager;
        private VisualEffectTransferModule _visualEffectTransfer;
        private ComputeShaderTransferModule _computeShaderTransfer;
        private WordManagerModule _wordManagerModule;

        private void Start()
        {
            #region 初期化
            VisualEffect vfx = GetComponent<VisualEffect>();
            World world = World.DefaultGameObjectInjectionWorld;
            ConstructModule(vfx, world);
            DependencyInjection();
            _gpuBufferContainer.InitializeData(_wordManagerModule, _initialSphereModule);
            #endregion

            _visualEffectTransfer.Play();

        }

        private void Update()
        {
            _entityManager.UpdateSystems();

            GlobalState globalState = _entityManager.GetGlobalState();
            ReadOnlySpan<int> phaseCounts = globalState.PhaseCountArray.AsReadOnlySpan();
            _computeShaderTransfer.Dispatch(Time.deltaTime, _gpuBufferContainer.PhaseIndicesBuffers, phaseCounts);
        }

        private void OnDestroy()
        {
            _gpuBufferContainer.Dispose();
            _entityManager.Dispose();
        }

        private void OnDrawGizmos()
        {
            InitialSphereModule.OnDrawGizmos(_initialRadius, _centerPosition);
            WordManagerModule.OnDrawGizmos(_wordDataArray);
        }

        /// <summary>
        ///     モジュールを生成。
        /// </summary>
        /// <param name="vfx"></param>
        private void ConstructModule(VisualEffect vfx, World world)
        {
            _wordManagerModule = new(_wordDataArray);
            _initialSphereModule = new(_initialRadius, _centerPosition);
            _gpuBufferContainer = new(_particleCount, _computeShaderData.KernelDastas.Length);
            _entityManager = new(world);
            _visualEffectTransfer = new(vfx, _vfxParameterNames);
            _computeShaderTransfer = new(_computeShaderData);
        }

        /// <summary>
        ///     依存性を注入。
        /// </summary>
        private void DependencyInjection()
        {
            _visualEffectTransfer.BindParameter(_gpuBufferContainer.PositionBuffer, _particleCount);
            _computeShaderTransfer.BindParameter(_gpuBufferContainer, _particleCount,
                _centerPosition, _phase1Configs.RotationRadius,
                _particleSpeed, _particleStopDistance);

            GPUBufferContainerLocator.Register(_gpuBufferContainer);
            _entityManager.CreateSystems(_particleCount, _computeShaderData.KernelDastas.Length, _phase1Configs);
        }
    }
}
