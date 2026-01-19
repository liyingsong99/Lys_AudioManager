using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lys.Audio
{
    [Serializable]
    public class ProbabilityCondition : IAudioPlayCondition
    {
        [TitleGroup("概率播放")]
        [Range(0f, 100f)]
        [LabelText("播放概率（%）")]
        public float probability = 100f;

        [TitleGroup("概率播放")]
        [LabelText("使用种子")]
        public bool useSeed = false;

        [TitleGroup("概率播放")]
        [ShowIf("useSeed")]
        [LabelText("随机种子")]
        public int seed = 0;

        [NonSerialized]
        private System.Random seededRandom;

        public string ConditionName => "概率播放";
        public string Description => $"概率: {probability}%";

        public bool Evaluate(AudioConditionContext context)
        {
            if (probability >= 100f) return true;
            if (probability <= 0f) return false;

            float roll;
            if (useSeed)
            {
                if (seededRandom == null)
                {
                    seededRandom = new System.Random(seed);
                }
                roll = (float)seededRandom.NextDouble() * 100f;
            }
            else
            {
                roll = UnityEngine.Random.Range(0f, 100f);
            }

            return roll < probability;
        }

        public void OnPlayStart(AudioConditionContext context) { }
        public void OnPlayStop(AudioConditionContext context) { }

        public void ResetSeed()
        {
            seededRandom = null;
        }
    }
}
