using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// 音效参数配置
    /// </summary>
    [Serializable]
    public class AudioClipParameters
    {
        [TitleGroup("基础参数")]
        [Range(0f, 1f)]
        [LabelText("音量")]
        public float volume = 1f;

        [Range(0.1f, 3f)]
        [LabelText("音调")]
        public float pitch = 1f;

        [TitleGroup("淡入淡出")]
        [Range(0f, 10f)]
        [LabelText("淡入时间(秒)")]
        public float fadeInTime = 0f;

        [Range(0f, 10f)]
        [LabelText("淡出时间(秒)")]
        public float fadeOutTime = 0f;

        [TitleGroup("播放设置")]
        [LabelText("循环播放")]
        public bool loop = false;

        [TitleGroup("空间音效")]
        [Range(0f, 1f)]
        [LabelText("空间混合度")]
        public float spatialBlend = 0f;

        [ShowIf("@spatialBlend > 0")]
        [Range(0f, 500f)]
        [LabelText("最小距离")]
        public float minDistance = 1f;

        [ShowIf("@spatialBlend > 0")]
        [Range(0f, 1000f)]
        [LabelText("最大距离")]
        public float maxDistance = 500f;

        [ShowIf("@spatialBlend > 0")]
        [Range(0f, 5f)]
        [LabelText("多普勒效果")]
        public float dopplerLevel = 1f;

        [ShowIf("@spatialBlend > 0")]
        [Range(0f, 360f)]
        [LabelText("扩散角度")]
        public float spread = 0f;

        [TitleGroup("其他设置")]
        [Range(0, 256)]
        [LabelText("优先级")]
        public int priority = 128;

        [TitleGroup("其他设置")]
        [LabelText("忽略监听器暂停")]
        public bool ignoreListenerPause = false;

        public static AudioClipParameters Default => new AudioClipParameters();

        public AudioClipParameters Clone()
        {
            return new AudioClipParameters
            {
                volume = this.volume,
                pitch = this.pitch,
                fadeInTime = this.fadeInTime,
                fadeOutTime = this.fadeOutTime,
                loop = this.loop,
                spatialBlend = this.spatialBlend,
                minDistance = this.minDistance,
                maxDistance = this.maxDistance,
                dopplerLevel = this.dopplerLevel,
                spread = this.spread,
                priority = this.priority,
                ignoreListenerPause = this.ignoreListenerPause
            };
        }

        public void ApplyTo(AudioSource source)
        {
            if (source == null) return;

            source.volume = volume;
            source.pitch = pitch;
            source.loop = loop;
            source.spatialBlend = spatialBlend;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.dopplerLevel = dopplerLevel;
            source.spread = spread;
            source.priority = priority;
            source.ignoreListenerPause = ignoreListenerPause;
        }
    }
}
