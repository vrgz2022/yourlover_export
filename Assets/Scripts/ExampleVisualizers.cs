// ExampleVisualizers - 音频可视化示例集合
// 展示如何使用音频数据实现多种可视化效果
using UnityEngine;
using Assets.WasapiAudio.Scripts.Core;
using Assets.WasapiAudio.Scripts.Unity;

public class ExampleVisualizer : MonoBehaviour
{
    // 可视化效果类型
    public enum VisualizerType
    {
        Particle,    // 粒子系统
        Light,      // 灯光强度
        Material,   // 材质颜色
        Rotation    // 物体旋转
    }
    
    [Header("音频设置")]
    public WasapiAudioSource wasapiSource;
    [Range(0.1f, 5f)]
    public float amplification = 2f;
    
    [Header("可视化器设置")]
    public VisualizerType visualizerType;
    
    [Header("通用参数")]
    public float smoothSpeed = 5f;
    
    [Header("粒子系统设置")]
    public ParticleSystem particleSystem;
    public float minParticleSize = 1f;
    public float maxParticleSize = 5f;
    
    [Header("灯光设置")]
    public Light targetLight;
    public float minIntensity = 0.5f;
    public float maxIntensity = 3f;
    
    [Header("材质设置")]
    public Renderer targetRenderer;
    public Color baseColor = Color.blue;
    public float colorIntensity = 1f;
    
    [Header("旋转设置")]
    public Vector3 rotationAxis = Vector3.up;
    public float minRotationSpeed = 30f;
    public float maxRotationSpeed = 180f;
    
    // 私有变量
    private Material _material;
    private Color _currentColor;
    private float _currentRotationSpeed;
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
        
        // 材质相关初始化
        if (visualizerType == VisualizerType.Material && targetRenderer != null)
        {
            _material = targetRenderer.material;
            _currentColor = baseColor;
        }
    }
    
    void Update()
    {
        switch (visualizerType)
        {
            case VisualizerType.Particle:
                UpdateParticle(_normalizedValue);
                break;
                
            case VisualizerType.Light:
                UpdateLight(_normalizedValue);
                break;
                
            case VisualizerType.Material:
                UpdateMaterial(_normalizedValue);
                break;
                
            case VisualizerType.Rotation:
                UpdateRotation(_normalizedValue);
                break;
        }
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
    
    // 更新粒子系统
    private void UpdateParticle(float normalizedValue)
    {
        if (particleSystem == null) return;
        
        float targetSize = Mathf.Lerp(minParticleSize, maxParticleSize, normalizedValue);
        var main = particleSystem.main;
        main.startSize = Mathf.Lerp(main.startSize.constant, targetSize, Time.deltaTime * smoothSpeed);
    }
    
    // 更新灯光
    private void UpdateLight(float normalizedValue)
    {
        if (targetLight == null) return;
        
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, normalizedValue);
        targetLight.intensity = Mathf.Lerp(targetLight.intensity, targetIntensity, Time.deltaTime * smoothSpeed);
    }
    
    // 更新材质
    private void UpdateMaterial(float normalizedValue)
    {
        if (_material == null) return;
        
        Color targetColor = baseColor * (1f + normalizedValue * colorIntensity);
        _currentColor = Color.Lerp(_currentColor, targetColor, Time.deltaTime * smoothSpeed);
        _material.color = _currentColor;
    }
    
    // 更新旋转
    private void UpdateRotation(float normalizedValue)
    {
        float targetSpeed = Mathf.Lerp(minRotationSpeed, maxRotationSpeed, normalizedValue);
        _currentRotationSpeed = Mathf.Lerp(_currentRotationSpeed, targetSpeed, Time.deltaTime * smoothSpeed);
        transform.Rotate(rotationAxis, _currentRotationSpeed * Time.deltaTime);
    }
    
    // 在Inspector中验证必要组件
    private void OnValidate()
    {
        switch (visualizerType)
        {
            case VisualizerType.Particle:
                if (particleSystem == null)
                    particleSystem = GetComponent<ParticleSystem>();
                break;
                
            case VisualizerType.Light:
                if (targetLight == null)
                    targetLight = GetComponent<Light>();
                break;
                
            case VisualizerType.Material:
                if (targetRenderer == null)
                    targetRenderer = GetComponent<Renderer>();
                break;
        }
    }
} 