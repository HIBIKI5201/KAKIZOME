using Master.Modules;
using Unity.Entities;
using UnityEngine;

namespace Master.Entities
{
    /// <summary>
    ///     GPUバッファコンテナを示すマーカーコンポーネント。
    /// </summary>
    public static class GPUBufferContainerLocator
    {
        public static IGraphicBufferContainer Get()
        {
            if (_gpuBufferContainer == null)
            {
                Debug.LogError("GPUBufferContainerLocator: GPUバッファコンテナが登録されていません。");
            }
            return _gpuBufferContainer;
        }

        public static void Register(IGraphicBufferContainer container)
        {
            _gpuBufferContainer = container;
        }

        private static IGraphicBufferContainer _gpuBufferContainer;
    }
}