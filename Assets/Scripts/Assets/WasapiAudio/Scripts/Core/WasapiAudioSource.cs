using UnityEngine;

namespace Assets.WasapiAudio.Scripts.Core
{
    public class WasapiAudioSource : MonoBehaviour
    {
        public WasapiCaptureType CaptureType;
        public void AddReceiver(object receiver) { }
    }

    public enum WasapiCaptureType
    {
        Loopback
    }
}

namespace Assets.WasapiAudio.Scripts.Unity
{
    public class SpectrumReceiver
    {
        public SpectrumReceiver(int size, object scaling, object window, int minFreq, int maxFreq, System.Action<float[]> callback) { }
    }

    public enum ScalingStrategy
    {
        Sqrt
    }

    public enum WindowFunctionType
    {
        BlackmannHarris
    }
} 