using Master.Modules;
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

        public static void Unregister(IGraphicBufferContainer container)
        {
            // 登録されているコンテナを渡さないと解除できない。
            if (container != _gpuBufferContainer) { return; }

            _gpuBufferContainer = null;
        }


        private static IGraphicBufferContainer _gpuBufferContainer;
    }
}