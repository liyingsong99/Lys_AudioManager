using System;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// 音效播放请求数据
    /// </summary>
    public class AudioPlayRequest
    {
        public string ClipName { get; set; }
        public AudioClipParameters Parameters { get; set; }
        public Vector3? Position { get; set; }
        public Transform FollowTarget { get; set; }
        public float Delay { get; set; }
        public Action OnComplete { get; set; }

        public static AudioPlayRequest Create(string clipName)
        {
            return new AudioPlayRequest { ClipName = clipName };
        }

        public static AudioPlayRequest Create3D(string clipName, Vector3 position)
        {
            return new AudioPlayRequest
            {
                ClipName = clipName,
                Position = position
            };
        }

        public static AudioPlayRequest CreateFollow(string clipName, Transform target)
        {
            return new AudioPlayRequest
            {
                ClipName = clipName,
                FollowTarget = target
            };
        }
    }
}
