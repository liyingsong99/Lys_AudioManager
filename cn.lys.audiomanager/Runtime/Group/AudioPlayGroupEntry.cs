using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// 播放组模式
    /// </summary>
    public enum PlayGroupMode
    {
        [LabelText("随机")]
        Random = 0,

        [LabelText("顺序")]
        Sequential = 1,

        [LabelText("互斥")]
        Exclusive = 2
    }

    /// <summary>
    /// 互斥播放行为 - 当同组已有音效播放时的处理方式
    /// </summary>
    public enum ExclusiveBehavior
    {
        [LabelText("不播放")]
        DontPlay = 0,

        [LabelText("停止旧的")]
        StopOld = 1,

        [LabelText("淡出旧的")]
        FadeOutOld = 2
    }

    /// <summary>
    /// 单个播放组配置
    /// </summary>
    [Serializable]
    public class AudioPlayGroupEntry
    {
        [LabelText("组名称")]
        [LabelWidth(60)]
        public string groupName;

        [EnumToggleButtons]
        [LabelText("播放模式")]
        public PlayGroupMode mode = PlayGroupMode.Random;

        [ShowIf("mode", PlayGroupMode.Exclusive)]
        [EnumToggleButtons]
        [LabelText("互斥行为")]
        [Tooltip("当同组已有音效播放时的处理方式")]
        public ExclusiveBehavior exclusiveBehavior = ExclusiveBehavior.DontPlay;

        [NonSerialized]
        public int currentSequenceIndex = 0;

        [NonSerialized]
        private System.Random randomGenerator;

        public string SelectNextClip(List<string> members, string requestedClip = null)
        {
            if (members == null || members.Count == 0)
            {
                return null;
            }

            if (members.Count == 1)
            {
                return members[0];
            }

            switch (mode)
            {
                case PlayGroupMode.Random:
                    if (randomGenerator == null)
                    {
                        randomGenerator = new System.Random();
                    }
                    return members[randomGenerator.Next(members.Count)];

                case PlayGroupMode.Sequential:
                    if (!string.IsNullOrEmpty(requestedClip))
                    {
                        int requestedIndex = members.IndexOf(requestedClip);
                        if (requestedIndex >= 0)
                        {
                            currentSequenceIndex = requestedIndex + 1;
                            return requestedClip;
                        }
                    }
                    var clip = members[currentSequenceIndex % members.Count];
                    currentSequenceIndex++;
                    return clip;

                case PlayGroupMode.Exclusive:
                    if (!string.IsNullOrEmpty(requestedClip) && members.Contains(requestedClip))
                    {
                        return requestedClip;
                    }
                    return members[0];

                default:
                    return members[0];
            }
        }

        public void ResetSequence()
        {
            currentSequenceIndex = 0;
        }
    }
}
