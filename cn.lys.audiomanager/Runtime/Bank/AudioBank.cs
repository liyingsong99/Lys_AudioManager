using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// 音效库 - 音效资源的容器
    /// </summary>
    [CreateAssetMenu(menuName = "Audio/Audio Bank", fileName = "NewAudioBank")]
    public class AudioBank : ScriptableObject
    {
        [TitleGroup("基础信息")]
        [LabelText("Bank 名称")]
        [PropertyOrder(-10)]
        [SerializeField]
        private string bankName;

        [TitleGroup("基础信息")]
        [LabelText("缓存类型")]
        [EnumToggleButtons]
        [SerializeField]
        private AudioBankCacheType cacheType = AudioBankCacheType.OnDemand;

        [TitleGroup("默认参数")]
        [LabelText("Bank 级别默认参数")]
        [InlineProperty]
        [HideLabel]
        [SerializeField]
        private AudioClipParameters defaultParameters = new AudioClipParameters();

        [TitleGroup("音效列表")]
        [LabelText("单个音效")]
        [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowItemCount = true)]
        [SerializeField]
        private List<AudioClipEntry> clipEntries = new List<AudioClipEntry>();

        [TitleGroup("文件夹索引")]
        [LabelText("文件夹音效")]
        [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)]
        [SerializeField]
        private List<AudioFolderEntry> folderEntries = new List<AudioFolderEntry>();

        #region Properties

        public string BankName
        {
            get => string.IsNullOrEmpty(bankName) ? name : bankName;
            set => bankName = value;
        }

        public AudioBankCacheType CacheType
        {
            get => cacheType;
            set => cacheType = value;
        }

        public AudioClipParameters DefaultParameters => defaultParameters;

        public IReadOnlyList<AudioClipEntry> ClipEntries => clipEntries;

        public IReadOnlyList<AudioFolderEntry> FolderEntries => folderEntries;

        #endregion

        #region Public Methods

        public IEnumerable<AudioClipEntry> GetAllClips()
        {
            foreach (var entry in clipEntries)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.clipName))
                {
                    yield return entry;
                }
            }

            foreach (var folder in folderEntries)
            {
                if (folder?.scannedClips == null) continue;

                foreach (var entry in folder.scannedClips)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.clipName))
                    {
                        yield return entry;
                    }
                }
            }
        }

        public AudioClipEntry GetClip(string clipName)
        {
            if (string.IsNullOrEmpty(clipName)) return null;

            foreach (var entry in clipEntries)
            {
                if (entry?.clipName == clipName)
                {
                    return entry;
                }
            }

            foreach (var folder in folderEntries)
            {
                if (folder?.scannedClips == null) continue;

                foreach (var entry in folder.scannedClips)
                {
                    if (entry?.clipName == clipName)
                    {
                        return entry;
                    }
                }
            }

            return null;
        }

        public bool Contains(string clipName)
        {
            return GetClip(clipName) != null;
        }

        public int GetClipCount()
        {
            int count = clipEntries?.Count ?? 0;

            if (folderEntries != null)
            {
                foreach (var folder in folderEntries)
                {
                    count += folder?.scannedClips?.Count ?? 0;
                }
            }

            return count;
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        [TitleGroup("基础信息")]
        [Button("注册到 AudioManager")]
        [GUIColor(0.4f, 0.8f, 0.4f)]
        [PropertyOrder(-5)]
        private void RegisterToManager()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[AudioBank] 只能在运行时注册到 AudioManager");
                return;
            }

            AudioManager.Instance.RegisterBank(this);
        }

        [TitleGroup("音效列表")]
        [Button("添加音效")]
        [GUIColor(0.4f, 0.6f, 0.8f)]
        private void AddClipEntry()
        {
            clipEntries.Add(new AudioClipEntry());
        }

        [TitleGroup("文件夹索引")]
        [Button("添加文件夹")]
        [GUIColor(0.4f, 0.6f, 0.8f)]
        private void AddFolderEntry()
        {
            folderEntries.Add(new AudioFolderEntry());
        }

        [TitleGroup("文件夹索引")]
        [Button("扫描所有文件夹")]
        [GUIColor(0.8f, 0.6f, 0.4f)]
        private void ScanAllFolders()
        {
            Debug.Log("[AudioBank] 扫描所有文件夹功能将在 Editor 模块实现");
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(bankName))
            {
                bankName = name;
            }
        }
#endif

        #endregion
    }
}
