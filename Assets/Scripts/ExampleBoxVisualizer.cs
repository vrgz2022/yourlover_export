// ExampleBoxVisualizer - 音频可视化示例
// 这个脚本展示如何创建一个随音乐节奏变化高度的盒子
using UnityEngine;
using Assets.WasapiAudio.Scripts.Core;
using Assets.WasapiAudio.Scripts.Unity;

public class ExampleBoxVisualizer : MonoBehaviour
{
    [Header("音频设置")]
    public WasapiAudioSource wasapiSource;
    [Range(0.1f, 5f)]
    public float amplification = 2f;

    [Header("高度设置")]
    [Tooltip("最小高度")]
    public float minHeight = 1f;
    [Tooltip("最大高度")]
    public float maxHeight = 20f;
    [Tooltip("平滑过渡速度")]
    public float smoothSpeed = 5f;
    
    [Header("颜色设置")]
    [Tooltip("是否启用颜色变化")]
    public bool enableColorChange = true;
    [Tooltip("基础颜色")]
    public Color baseColor = Color.blue;
    [Tooltip("颜色变化强度")]
    public float colorIntensity = 1f;
    
    private Vector3 _targetScale;
    private Material _material;
    private Color _targetColor;
    private Vector3 _originalScale;
    private float _normalizedValue;
    private SpectrumReceiver _receiver;
    
    void Start()
    {
        // 初始化音频接收器
        if (wasapiSource == null)
        {
            wasapiSource = GetComponent<WasapiAudioSource>();
        }
        
        if (wasapiSource == null)
        {
            wasapiSource = gameObject.AddComponent<WasapiAudioSource>();
            wasapiSource.CaptureType = WasapiCaptureType.Loopback;
        }
        
        _receiver = new SpectrumReceiver(
            1024,
            ScalingStrategy.Sqrt,
            WindowFunctionType.BlackmannHarris,
            20,
            20000,
            OnSpectrumDataReceived
        );
        
        wasapiSource.AddReceiver(_receiver);
        
        // 保存原始缩放
        _originalScale = transform.localScale;
        _targetScale = _originalScale;
        
        // 获取材质
        _material = GetComponent<Renderer>().material;
        _targetColor = baseColor;
    }
    
    // 音频数据处理
    private void OnSpectrumDataReceived(float[] spectrumData)
    {
        if (spectrumData == null) return;
        
        // 计算平均强度并标准化到0-1
        float sum = 0;
        for (int i = 0; i < spectrumData.Length; i++)
        {
            sum += spectrumData[i];
        }
        _normalizedValue = Mathf.Clamp01((sum / spectrumData.Length) * amplification);
    }
    
    void Update()
    {
        // 更新高度
        float targetHeight = Mathf.Lerp(minHeight, maxHeight, _normalizedValue);
        _targetScale = new Vector3(_originalScale.x, targetHeight * _originalScale.y, _originalScale.z);
        transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * smoothSpeed);
        
        // 更新颜色
        if (enableColorChange && _material != null)
        {
            float colorValue = Mathf.Lerp(0, colorIntensity, _normalizedValue);
            _targetColor = baseColor * (1 + colorValue);
            _material.color = Color.Lerp(_material.color, _targetColor, Time.deltaTime * smoothSpeed);
        }
    }
} 