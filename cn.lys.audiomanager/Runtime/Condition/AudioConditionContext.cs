using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// 音效条件评估上下文
    /// </summary>
    public class AudioConditionContext
    {
        public string ClipName { get; set; }
        public AudioClipEntry ClipEntry { get; set; }
        public AudioBank Bank { get; set; }
        public AudioGroup Group { get; set; }
        public AudioPlayRequest Request { get; set; }
        public AudioManager Manager { get; set; }
        public int CurrentPlayingCount { get; set; }
        public float LastPlayTime { get; set; }
        public float CurrentTime => Time.time;
    }
}
