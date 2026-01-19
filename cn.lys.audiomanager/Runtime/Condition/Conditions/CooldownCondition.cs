using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lys.Audio
{
    [Serializable]
    public class CooldownCondition : IAudioPlayCondition
    {
        [TitleGroup("冷却时间")]
        [MinValue(0)]
        [LabelText("冷却时间（秒）")]
        public float cooldownTime = 0.1f;

        [TitleGroup("冷却时间")]
        [LabelText("冷却期间行为")]
        [EnumToggleButtons]
        public CooldownBehavior behavior = CooldownBehavior.Skip;

        public string ConditionName => "冷却时间";
        public string Description => $"冷却: {cooldownTime}s, 行为: {behavior}";

        public bool Evaluate(AudioConditionContext context)
        {
            if (cooldownTime <= 0) return true;

            float timeSinceLastPlay = Time.time - context.LastPlayTime;

            if (timeSinceLastPlay < cooldownTime)
            {
                switch (behavior)
                {
                    case CooldownBehavior.Skip:
                        return false;

                    case CooldownBehavior.Queue:
                        return false;

                    default:
                        return false;
                }
            }

            return true;
        }

        public void OnPlayStart(AudioConditionContext context) { }
        public void OnPlayStop(AudioConditionContext context) { }
    }

    public enum CooldownBehavior
    {
        Skip = 0,
        Queue = 1
    }
}
