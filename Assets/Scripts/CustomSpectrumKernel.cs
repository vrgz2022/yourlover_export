// CustomSpectrumKernel - 自定义音频数据核心
// 提供标准化的音频数据给创意工坊内容使用
using UnityEngine;
using Assets.WasapiAudio.Scripts.Core;
using Assets.WasapiAudio.Scripts.Unity;

public class CustomSpectrumKernel : MonoBehaviour
{
    // 静态数据接口
    public static float normalizedValue = 0f;  // 0-1的标准化音频强度值
    
    // 参数设置
    [Range(0.1f, 5f)]
    public float amplification = 2f;
    
    // 引用
    private WasapiAudioSource _wasapiSource;
    
    void Start()
    {
        // 获取或添加WasapiAudioSource组件
        _wasapiSource = GetComponent<WasapiAudioSource>();
        if (_wasapiSource == null)
        {
            _wasapiSource = gameObject.AddComponent<WasapiAudioSource>();
            _wasapiSource.CaptureType = WasapiCaptureType.Loopback;
        }
        
        // 设置接收器
        var receiver = new SpectrumReceiver(
            1024,
            ScalingStrategy.Sqrt,
            WindowFunctionType.BlackmannHarris,
            20,
            20000,
            OnSpectrumDataReceived
        );
        
        // 添加到WasapiAudioSource
        _wasapiSource.AddReceiver(receiver);
    }
    
    // 数据处理回调
    private void OnSpectrumDataReceived(float[] spectrumData)
    {
        if (spectrumData == null) return;
        
        // 计算平均值
        float sum = 0;
        for (int i = 0; i < spectrumData.Length; i++)
        {
            sum += spectrumData[i];
        }
        
        // 更新静态值
        normalizedValue = Mathf.Clamp01(sum / spectrumData.Length * amplification);
    }
} 