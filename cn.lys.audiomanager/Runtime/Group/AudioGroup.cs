using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace Lys.Audio
{
    /// <summary>
    /// 音效组 - AudioBank 的容器，关联 AudioMixerGroup
    /// </summary>
    [CreateAssetMenu(menuName = "Audio/Audio Group", fileName = "NewAudioGroup")]
    public class AudioGroup : ScriptableObject
    {
        [TitleGroup("基础设置")]
        [LabelText("组名称")]
        [PropertyOrder(-10)]
        [SerializeField]
        private string groupName;

        [TitleGroup("基础设置")]
        [LabelText("Mixer Group")]
        [Tooltip("默认的 AudioMixerGroup，组内所有音效使用此 MixerGroup")]
        [SerializeField]
        private AudioMixerGroup mixerGroup;

        [TitleGroup("关联音效库")]
        [LabelText("Audio Banks")]
        [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowItemCount = true)]
        [SerializeField]
        private List<AudioBank> audioBanks = new List<AudioBank>();

        [TitleGroup("组级别设置")]
        [LabelText("组默认设置")]
        [InlineProperty]
        [HideLabel]
        [SerializeField]
        private AudioGroupSettings groupSettings = new AudioGroupSettings();

        #region Properties

        public string GroupName
        {
            get => string.IsNullOrEmpty(groupName) ? name : groupName;
            set => groupName = value;
        }

        public AudioMixerGroup MixerGroup
        {
            get => mixerGroup;
            set => mixerGroup = value;
        }

        public IReadOnlyList<AudioBank> AudioBanks => audioBanks;

        public AudioGroupSettings Settings => groupSettings;

        #endregion

        #region Public Methods

        public IEnumerable<AudioClipEntry> GetAllClips()
        {
            foreach (var bank in audioBanks)
            {
                if (bank == null) continue;

                foreach (var entry in bank.GetAllClips())
                {
                    yield return entry;
                }
            }
        }

        public AudioClipEntry GetClip(string clipName)
        {
            if (string.IsNullOrEmpty(clipName)) return null;

            foreach (var bank in audioBanks)
            {
                if (bank == null) continue;

                var entry = bank.GetClip(clipName);
                if (entry != null)
                {
                    return entry;
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
            int count = 0;
            foreach (var bank in audioBanks)
            {
                if (bank != null)
                {
                    count += bank.GetClipCount();
                }
            }
            return count;
        }

        public void AddBank(AudioBank bank)
        {
            if (bank == null) return;

            foreach (var b in audioBanks)
            {
                if (b == bank) return;
            }
            audioBanks.Add(bank);
        }

        public void RemoveBank(AudioBank bank)
        {
            audioBanks.Remove(bank);
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        [TitleGroup("基础设置")]
        [Button("注册到 AudioManager")]
        [GUIColor(0.4f, 0.8f, 0.4f)]
        [PropertyOrder(-5)]
        private void RegisterToManager()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[AudioGroup] 只能在运行时注册到 AudioManager");
                return;
            }

            AudioManager.Instance.RegisterGroup(this);
        }

        [TitleGroup("编辑器")]
        [Button("打开音效编辑器", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.6f, 0.8f)]
        private void OpenAudioEditor()
        {
            Debug.Log($"[AudioGroup] 打开音效编辑器: {GroupName}");

            var editorAssembly = System.Reflection.Assembly.Load("AudioManager.Editor");
            if (editorAssembly != null)
            {
                var windowType = editorAssembly.GetType("Lys.Audio.Editor.AudioEditorWindow");
                if (windowType != null)
                {
                    var openMethod = windowType.GetMethod("Open", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    openMethod?.Invoke(null, new object[] { this });
                }
            }
        }

        [TitleGroup("关联音效库")]
        [Button("添加新 Bank")]
        [GUIColor(0.4f, 0.6f, 0.8f)]
        private void AddNewBank()
        {
            audioBanks.Add(null);
        }

        [TitleGroup("统计信息")]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("Bank 数量")]
        private int BankCount => audioBanks?.Count ?? 0;

        [TitleGroup("统计信息")]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("音效总数")]
        private int TotalClipCount => GetClipCount();

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(groupName))
            {
                groupName = name;
            }
        }
#endif

        #endregion
    }
}
