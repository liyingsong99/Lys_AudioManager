using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lys.Audio
{
    [Serializable]
    public class ConcurrentLimitCondition : IAudioPlayCondition
    {
        [TitleGroup("同时播放上限")]
        [Range(1, 32)]
        [LabelText("最大同时播放数")]
        public int maxConcurrent = 1;

        [TitleGroup("同时播放上限")]
        [EnumToggleButtons]
        [LabelText("超出限制行为")]
        public OverLimitBehavior overLimitBehavior = OverLimitBehavior.DontPlay;

        public string ConditionName => "同时播放上限";
        public string Description => $"上限: {maxConcurrent}, 行为: {overLimitBehavior}";

        public bool Evaluate(AudioConditionContext context)
        {
            if (maxConcurrent <= 0) return true;

            int currentCount = context.CurrentPlayingCount;

            if (currentCount < maxConcurrent)
            {
                return true;
            }

            switch (overLimitBehavior)
            {
                case OverLimitBehavior.DontPlay:
                    return false;

                case OverLimitBehavior.StopOldest:
                    StopOldestInstance(context);
                    return true;

                case OverLimitBehavior.StopLowestPriority:
                    StopLowestPriorityInstance(context);
                    return true;

                default:
                    return false;
            }
        }

        private void StopOldestInstance(AudioConditionContext context)
        {
            var manager = context.Manager;
            if (manager == null) return;

            ActiveAudioInstance oldest = null;
            float earliestStartTime = float.MaxValue;

            foreach (var instance in manager.GetActiveInstances())
            {
                if (instance == null || !instance.IsPlaying) continue;
                if (instance.ClipName != context.ClipName) continue;

                if (instance.StartTime < earliestStartTime)
                {
                    earliestStartTime = instance.StartTime;
                    oldest = instance;
                }
            }

            oldest?.Stop(true);
        }

        private void StopLowestPriorityInstance(AudioConditionContext context)
        {
            var manager = context.Manager;
            if (manager == null) return;

            ActiveAudioInstance lowestPriority = null;
            int lowestPriorityValue = int.MaxValue;

            foreach (var instance in manager.GetActiveInstances())
            {
                if (instance == null || !instance.IsPlaying) continue;
                if (instance.ClipName != context.ClipName) continue;

                int priority = GetInstancePriority(instance, context);
                if (priority < lowestPriorityValue)
                {
                    lowestPriorityValue = priority;
                    lowestPriority = instance;
                }
            }

            lowestPriority?.Stop(true);
        }

        private int GetInstancePriority(ActiveAudioInstance instance, AudioConditionContext context)
        {
            if (instance?.Source != null)
            {
                return 256 - instance.Source.priority;
            }
            return 128;
        }

        public void OnPlayStart(AudioConditionContext context) { }
        public void OnPlayStop(AudioConditionContext context) { }
    }
}
