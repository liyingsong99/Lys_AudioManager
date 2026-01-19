using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// 单个音效条目
    /// </summary>
    [Serializable]
    public class AudioClipEntry
    {
        [HorizontalGroup("Main", 0.3f)]
        [LabelText("音效名称")]
        [LabelWidth(60)]
        public string clipName;

        [HorizontalGroup("Main")]
        [LabelText("资源路径")]
        [LabelWidth(60)]
        [Tooltip("YooAsset 资源路径，如 Audio/SFX/click")]
        public string assetPath;

        [FoldoutGroup("事件配置")]
        [LabelText("音效事件名")]
        [Tooltip("可选的事件名，用于通过事件名播放此音效。为空时只能通过 clipName 播放")]
        public string eventName;

        [FoldoutGroup("参数覆盖")]
        [LabelText("使用自定义参数")]
        public bool useCustomParameters = false;

        [FoldoutGroup("参数覆盖")]
        [ShowIf("useCustomParameters")]
        [InlineProperty]
        [HideLabel]
        public AudioClipParameters customParameters = new AudioClipParameters();

        [FoldoutGroup("播放组")]
        [LabelText("所属播放组")]
        [ValueDropdown("GetPlayGroupNames")]
        [Tooltip("音效所属的播放组。播放时会根据组规则（随机/顺序/互斥）选择实际播放的音效")]
        public string playGroupName;

        [FoldoutGroup("播放条件")]
        [LabelText("条件关系")]
        public AudioConditionOperator conditionOperator = AudioConditionOperator.And;

        [FoldoutGroup("播放条件")]
        [ListDrawerSettings(ShowFoldout = true)]
        [SerializeReference]
        public IAudioPlayCondition[] playConditions = Array.Empty<IAudioPlayCondition>();

#if UNITY_EDITOR
        private System.Collections.Generic.IEnumerable<string> GetPlayGroupNames()
        {
            var settings = AudioPlayGroupSettings.Instance;
            if (settings == null)
            {
                return new string[] { "" };
            }
            return settings.GetGroupNames();
        }
#endif

#if UNITY_EDITOR
        [HideInInspector]
        public AudioClip editorPreviewClip;

        [HorizontalGroup("Main", 60)]
        [Button("预览")]
        [GUIColor(0.4f, 0.8f, 0.4f)]
        private void PreviewInEditor()
        {
            Debug.Log($"[AudioManager] Preview: {clipName} ({assetPath})");
        }
#endif

        [NonSerialized]
        public AudioClip loadedClip;

        public bool IsLoaded => loadedClip != null;

        public AudioClipParameters GetParameters(AudioClipParameters bankDefault)
        {
            if (useCustomParameters && customParameters != null)
            {
                return customParameters;
            }
            return bankDefault ?? AudioClipParameters.Default;
        }
    }
}
