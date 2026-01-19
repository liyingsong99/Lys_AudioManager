using Sirenix.OdinInspector;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// AudioManager 全局设置
    /// </summary>
    [CreateAssetMenu(menuName = "Audio/Audio Manager Settings", fileName = "AudioManagerSettings")]
    public class AudioManagerSettings : ScriptableObject
    {
        private static AudioManagerSettings instance;

        public static AudioManagerSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<AudioManagerSettings>("AudioManagerSettings");
                    if (instance == null)
                    {
                        instance = CreateInstance<AudioManagerSettings>();
                    }
                }
                return instance;
            }
        }

        [TitleGroup("对象池设置")]
        [Range(8, 128)]
        [LabelText("最大 AudioSource 数量")]
        public int maxAudioSources = 32;

        [TitleGroup("对象池设置")]
        [Range(0, 32)]
        [LabelText("预热数量")]
        [Tooltip("启动时预先创建的 AudioSource 数量")]
        public int warmupCount = 8;

        [TitleGroup("日志设置")]
        [LabelText("启用调试日志")]
        public bool enableDebugLog = false;

        [TitleGroup("日志设置")]
        [LabelText("启用播放日志")]
        [Tooltip("记录每次音效播放")]
        public bool enablePlayLog = false;

        [TitleGroup("默认参数")]
        [InlineProperty]
        [HideLabel]
        public AudioClipParameters defaultParameters = new AudioClipParameters();

        [TitleGroup("资源加载")]
        [LabelText("默认加载超时(秒)")]
        [Range(1f, 30f)]
        public float loadTimeout = 10f;

        [TitleGroup("资源加载")]
        [LabelText("加载失败重试次数")]
        [Range(0, 5)]
        public int loadRetryCount = 2;
    }
}
