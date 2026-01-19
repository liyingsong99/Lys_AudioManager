using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// AudioGroup 级别的默认设置
    /// </summary>
    [Serializable]
    public class AudioGroupSettings
    {
        [TitleGroup("音量设置")]
        [Range(0f, 1f)]
        [LabelText("组音量")]
        public float groupVolume = 1f;

        [TitleGroup("音量设置")]
        [LabelText("静音")]
        public bool muted = false;

        [TitleGroup("播放限制")]
        [Range(0, 32)]
        [LabelText("最大同时播放数")]
        [Tooltip("0 表示不限制")]
        public int maxConcurrentSounds = 0;

        [TitleGroup("播放限制")]
        [LabelText("超出限制时的行为")]
        [EnumToggleButtons]
        public OverLimitBehavior overLimitBehavior = OverLimitBehavior.DontPlay;

        [TitleGroup("默认参数覆盖")]
        [LabelText("使用组级别参数")]
        public bool useGroupParameters = false;

        [TitleGroup("默认参数覆盖")]
        [ShowIf("useGroupParameters")]
        [InlineProperty]
        [HideLabel]
        public AudioClipParameters groupParameters = new AudioClipParameters();
    }

    /// <summary>
    /// 超出播放限制时的行为
    /// </summary>
    public enum OverLimitBehavior
    {
        DontPlay = 0,
        StopOldest = 1,
        StopLowestPriority = 2
    }
}
