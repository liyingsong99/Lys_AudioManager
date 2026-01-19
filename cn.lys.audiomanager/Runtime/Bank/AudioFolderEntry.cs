using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// 音效配置覆盖数据 - 用于持久化保存文件夹音效的自定义配置
    /// </summary>
    [Serializable]
    public class AudioClipOverrideData
    {
        public string eventName = "";
        public string playGroupName = "";
        public AudioConditionOperator conditionOperator = AudioConditionOperator.And;
        public bool useCustomParameters = false;
        public AudioClipParameters customParameters;

        [SerializeReference]
        public IAudioPlayCondition[] playConditions;
    }

    /// <summary>
    /// 文件夹音效条目 - 批量索引整个文件夹的音效
    /// </summary>
    [Serializable]
    public class AudioFolderEntry
    {
        [TitleGroup("文件夹配置")]
        [LabelText("文件夹路径")]
        [FolderPath(RequireExistingPath = true)]
        [Tooltip("Unity 项目中的文件夹路径")]
        public string folderPath;

        [TitleGroup("文件夹配置")]
        [LabelText("名称前缀")]
        [Tooltip("添加到所有音效名称前的前缀，如 'UI_' 会使 click 变成 UI_click")]
        public string namePrefix = "";

        [TitleGroup("文件夹配置")]
        [LabelText("包含子文件夹")]
        public bool includeSubfolders = false;

        [TitleGroup("默认参数")]
        [LabelText("使用自定义参数")]
        public bool useCustomParameters = false;

        [TitleGroup("默认参数")]
        [ShowIf("useCustomParameters")]
        [InlineProperty]
        [HideLabel]
        public AudioClipParameters customParameters = new AudioClipParameters();

        [SerializeField]
        [HideInInspector]
        private List<string> clipOverrideKeys = new List<string>();

        [SerializeField]
        [HideInInspector]
        private List<AudioClipOverrideData> clipOverrideValues = new List<AudioClipOverrideData>();

        [SerializeField]
        public List<AudioClipEntry> scannedClips = new List<AudioClipEntry>();

        public AudioClipOverrideData GetOverride(string clipName)
        {
            if (string.IsNullOrEmpty(clipName)) return null;

            int index = clipOverrideKeys.IndexOf(clipName);
            if (index >= 0 && index < clipOverrideValues.Count)
            {
                return clipOverrideValues[index];
            }
            return null;
        }

        public void SetOverride(string clipName, AudioClipOverrideData data)
        {
            if (string.IsNullOrEmpty(clipName) || data == null) return;

            int index = clipOverrideKeys.IndexOf(clipName);
            if (index >= 0)
            {
                clipOverrideValues[index] = data;
            }
            else
            {
                clipOverrideKeys.Add(clipName);
                clipOverrideValues.Add(data);
            }
        }

        public void RemoveOverride(string clipName)
        {
            if (string.IsNullOrEmpty(clipName)) return;

            int index = clipOverrideKeys.IndexOf(clipName);
            if (index >= 0)
            {
                clipOverrideKeys.RemoveAt(index);
                if (index < clipOverrideValues.Count)
                {
                    clipOverrideValues.RemoveAt(index);
                }
            }
        }

        public void ApplyOverridesToScannedClips()
        {
            if (scannedClips == null) return;

            foreach (var entry in scannedClips)
            {
                if (entry == null) continue;

                var overrideData = GetOverride(entry.clipName);
                if (overrideData == null) continue;

                entry.eventName = overrideData.eventName;
                entry.playGroupName = overrideData.playGroupName;
                entry.conditionOperator = overrideData.conditionOperator;
                entry.useCustomParameters = overrideData.useCustomParameters;

                if (overrideData.customParameters != null)
                {
                    entry.customParameters = overrideData.customParameters;
                }

                if (overrideData.playConditions != null)
                {
                    entry.playConditions = overrideData.playConditions;
                }
            }
        }

#if UNITY_EDITOR
        [TitleGroup("文件夹配置")]
        [Button("扫描文件夹")]
        [GUIColor(0.4f, 0.6f, 0.8f)]
        private void ScanFolder()
        {
            Debug.Log($"[AudioManager] Scan folder: {folderPath}");
        }

        [TitleGroup("文件夹配置")]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("已扫描音效数")]
        private int ScannedCount => scannedClips?.Count ?? 0;
#endif
    }
}
