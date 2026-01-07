// write by gpt.

using UnityEngine;
using System.Text;
using UnityEngine.InputSystem;
using Master.Configs;




#if UNITY_EDITOR
using UnityEditor;
#endif

public class RuntimeProfiler : MonoBehaviour, IParticleCountContainer
{
    public int ParticleCount => _particleCount;

    private int _particleCount = -1;
    private bool _isActive;
    private string _value = string.Empty;

    private StringBuilder _sb = new(256);
    private float _deltaTime;
    private FrameTiming[] _frameTimings = new FrameTiming[1];

    private void Awake()
    {
        // 自身が二つ以上あれば自身を破壊。
        if (IParticleCountContainer.Instance == null)
        {
            IParticleCountContainer.Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            _isActive = !_isActive;
        }

            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
    }

    private void OnDestroy()
    {
        if (IParticleCountContainer.Instance == (object)this)
        {
            IParticleCountContainer.Instance = null;
        }
    }

    private void OnGUI()
    {
        if (!_isActive) { return; }

        _sb.Clear();

        // FPS
        float fps = 1.0f / _deltaTime;
        _sb.AppendLine($"FPS : {fps:F1}");

        // CPU / GPU Frame Time（Runtime可）
        FrameTimingManager.CaptureFrameTimings();
        if (FrameTimingManager.GetLatestTimings(1, _frameTimings) > 0)
        {
            var timing = _frameTimings[0];
            _sb.AppendLine($"CPU Frame Time : {timing.cpuFrameTime:F2} ms");
            _sb.AppendLine($"GPU Frame Time : {timing.gpuFrameTime:F2} ms");
        }

#if UNITY_EDITOR
        // Editor専用描画統計
        _sb.AppendLine($"Draw Calls     : {UnityStats.drawCalls}");
        _sb.AppendLine($"Batches        : {UnityStats.batches}");
        _sb.AppendLine($"SetPass Calls  : {UnityStats.setPassCalls}");
        _sb.AppendLine($"Triangles      : {UnityStats.triangles}");
        _sb.AppendLine($"Vertices       : {UnityStats.vertices}");
#endif

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            wordWrap = true
        };

        float width = 420f;

        float height = style.CalcHeight(
            new GUIContent(_sb.ToString()),
            width
        );

        Rect sbRect = new Rect(10, 10, width, height);

        GUI.Label(sbRect, _sb.ToString());

        Rect valueRect = new(sbRect.x, sbRect.y + sbRect.height, sbRect.width, 30);
        string newValue = GUI.TextField(valueRect, _value);
        if (newValue != _value)
        {
            if (int.TryParse(newValue, out int count))
            {
                _particleCount = count;
            }

            _value = newValue;
        }
    }
}
