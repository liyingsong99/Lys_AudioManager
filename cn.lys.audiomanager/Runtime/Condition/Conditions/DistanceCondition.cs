using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lys.Audio
{
    [Serializable]
    public class DistanceCondition : IAudioPlayCondition
    {
        [TitleGroup("距离条件")]
        [LabelText("参考位置类型")]
        [EnumToggleButtons]
        public DistanceReferenceType referenceType = DistanceReferenceType.MainCamera;

        [TitleGroup("距离条件")]
        [ShowIf("referenceType", DistanceReferenceType.CustomTransform)]
        [LabelText("自定义参考点")]
        public Transform customReference;

        [TitleGroup("距离条件")]
        [MinValue(0)]
        [LabelText("最大距离")]
        public float maxDistance = 50f;

        [TitleGroup("距离条件")]
        [MinValue(0)]
        [LabelText("最小距离")]
        public float minDistance = 0f;

        public string ConditionName => "距离条件";
        public string Description => $"距离: {minDistance}-{maxDistance}, 参考: {referenceType}";

        public bool Evaluate(AudioConditionContext context)
        {
            Vector3? playPosition = null;

            if (!playPosition.HasValue)
            {
                return true;
            }

            Vector3 referencePosition = GetReferencePosition();
            float distance = Vector3.Distance(playPosition.Value, referencePosition);

            if (distance <= minDistance)
            {
                return true;
            }

            if (distance > maxDistance)
            {
                return false;
            }

            return true;
        }

        private Vector3 GetReferencePosition()
        {
            switch (referenceType)
            {
                case DistanceReferenceType.MainCamera:
                    return Camera.main != null ? Camera.main.transform.position : Vector3.zero;

                case DistanceReferenceType.AudioListener:
                    var listener = UnityEngine.Object.FindFirstObjectByType<AudioListener>();
                    return listener != null ? listener.transform.position : Vector3.zero;

                case DistanceReferenceType.CustomTransform:
                    return customReference != null ? customReference.position : Vector3.zero;

                default:
                    return Vector3.zero;
            }
        }

        public void OnPlayStart(AudioConditionContext context) { }
        public void OnPlayStop(AudioConditionContext context) { }
    }

    public enum DistanceReferenceType
    {
        MainCamera = 0,
        AudioListener = 1,
        CustomTransform = 2
    }
}
