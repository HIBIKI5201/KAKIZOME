using System;
using UnityEngine;
using UnityEngine.VFX;

namespace Master.Modules
{
    /// <summary>
    ///     VFX Graphとデータをやり取りするためのモジュール。
    /// </summary>
    public class VisualEffectTransferModule
    {
        public VisualEffectTransferModule(VisualEffect vfx, VFXParameterNames param)
        {
            // バリデーションチェック。
            Debug.Assert(vfx != null, "VisualEffect component is null.");
            VFXParameterNames.Assert(param);

            // メンバ変数に保存。
            _vfx = vfx;
            _paramNames = param;
        }

        /// <summary>
        ///     バッファやパーティクル数をVFX Graphにバインドする。
        /// </summary>
        /// <param name="positionBuffer"></param>
        /// <param name="count"></param>
        public void BindParameter(IGraphicBufferContainer container, int count)
        {
            // バッファとパーティクル数をVFXにバインド。
            _vfx.SetGraphicsBuffer(_paramNames.PositionBuffer, container.PositionBuffer);
            _vfx.SetGraphicsBuffer(_paramNames.ColorBuffer, container.ColorBuffer);
            _vfx.SetInt(_paramNames.ParticleCount, count);
        }

        /// <summary>
        ///     VFXを再生する。
        /// </summary>
        public void Play()
        {
            _vfx.Play();
        }

        private readonly VisualEffect _vfx;
        private readonly VFXParameterNames _paramNames;

        [Serializable]
        public struct VFXParameterNames
        {
            public string PositionBuffer => _positionBuffer;
            public string ParticleCount => _particleCount;
            public string ColorBuffer => _colorBuffer;

            public static void Assert(VFXParameterNames param)
            {
                Debug.Assert(!string.IsNullOrEmpty(param.PositionBuffer), "PointBuffer null or empty parameter names.");
                Debug.Assert(!string.IsNullOrEmpty(param.ParticleCount), "ParticleCount null or empty parameter names.");
            }

            [SerializeField, Tooltip("位置バッファのパラメータ名")]
            private string _positionBuffer;
            [SerializeField, Tooltip("パーティクル量のパラメータ名")]
            private string _particleCount;
            [SerializeField, Tooltip("色バッファのパラメータ名")]
            private string _colorBuffer;
        }
    }
}
