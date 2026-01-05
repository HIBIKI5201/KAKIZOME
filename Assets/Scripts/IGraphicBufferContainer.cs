using UnityEngine;

namespace Master.Modules
{
    /// <summary>
    /// グラフィックバッファコンテナのインターフェース。
    /// グラフィックバッファの管理とアクセスを提供する役割を担う。
    /// </summary>
    public interface IGraphicBufferContainer
    {
        public GraphicsBuffer PositionBuffer { get; }
        public GraphicsBuffer VelocityBuffer { get; }
        public GraphicsBuffer TargetBuffer { get; }
        public GraphicsBuffer ColorBuffer {  get; }
        public GraphicsBuffer[] PhaseIndicesBuffers { get; }
    }
}
