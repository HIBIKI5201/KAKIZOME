using Master.Configs;
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
        private DefaultParticleCount _defaultParticleCount;

        [Space]
        [SerializeField]
        private PhaseConfigRepository _phaseConfigRepository;

        [Header("モジュールパラメータ設定")]
        [SerializeField, Tooltip("VFXのパラメータ名")]
        private VisualEffectTransferModule.VFXParameterNames _vfxParameterNames;
        [SerializeField, Tooltip("コンピュートシェーダーのパラメータ")]
        private ComputeShaderTransferModule.ComputeShaderData _computeShaderData;
        [SerializeField, Tooltip("文字データ")]
        private WordManagerModule.WordData[] _wordDataArray;


        private int _particleCount;
        private InitialSphereModule _initialSphereModule;
        private GPUBufferContainerModule _gpuBufferContainer;
        private EntityManagerModule _entityManager;
        private VisualEffectTransferModule _visualEffectTransfer;
        private ComputeShaderTransferModule _computeShaderTransfer;
        private WordManagerModule _wordManagerModule;

        private void Start()
        {
            #region 初期化
            IParticleCountContainer particleCount = FindAnyObjectByType<RuntimeProfiler>();
            _particleCount = particleCount.GetParticleCount(_defaultParticleCount.Value);
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
            _computeShaderTransfer.Dispatch(Time.deltaTime, Time.time,
                _gpuBufferContainer.PhaseIndicesBuffers, phaseCounts,
                _particleCount);
        }

        private void OnDestroy()
        {
            GPUBufferContainerLocator.Unregister(_gpuBufferContainer);
            _gpuBufferContainer.Dispose();
            _entityManager.Dispose();
        }

        private void OnDrawGizmos()
        {
            InitialSphereModule.OnDrawGizmos(
                _phaseConfigRepository.Phase0Configs.InitialRadius,
                _phaseConfigRepository.GlobalConfigs.CenterPosition);
            WordManagerModule.OnDrawGizmos(_wordDataArray);
            Phase2Configs.OnDrawGizmos(
                _phaseConfigRepository.Phase2Configs,
                _phaseConfigRepository.GlobalConfigs.CenterPosition);
            Phase3Configs.OnDrawGizmos(
                _phaseConfigRepository.Phase3Configs,
                _phaseConfigRepository.GlobalConfigs.CenterPosition);
        }

        /// <summary>
        ///     モジュールを生成。
        /// </summary>
        /// <param name="vfx"></param>
        private void ConstructModule(VisualEffect vfx, World world)
        {
            _wordManagerModule = new(_wordDataArray);
            _initialSphereModule = new(
                _phaseConfigRepository.Phase0Configs.InitialRadius,
                _phaseConfigRepository.GlobalConfigs.CenterPosition);
            _gpuBufferContainer = new(_particleCount, _computeShaderData.PhaseKernelDastas.Length);
            _entityManager = new(world);
            _visualEffectTransfer = new(vfx, _vfxParameterNames);
            _computeShaderTransfer = new(_computeShaderData);
        }

        /// <summary>
        ///     依存性を注入。
        /// </summary>
        private void DependencyInjection()
        {
            _visualEffectTransfer.BindParameter(_gpuBufferContainer, _particleCount);
            _computeShaderTransfer.BindParameter(_gpuBufferContainer, _particleCount,
                _phaseConfigRepository);

            GPUBufferContainerLocator.Register(_gpuBufferContainer);
            _entityManager.CreateSystems(_particleCount,
                _computeShaderData.PhaseKernelDastas.Length, 
                _phaseConfigRepository);
        }
    }
}
