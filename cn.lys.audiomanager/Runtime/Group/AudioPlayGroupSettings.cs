using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// 播放组配置 - 集中管理所有播放组
    /// </summary>
    [CreateAssetMenu(menuName = "Audio/Audio Play Group Settings", fileName = "AudioPlayGroupSettings")]
    public class AudioPlayGroupSettings : ScriptableObject
    {
        private static AudioPlayGroupSettings instance;

        public static AudioPlayGroupSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<AudioPlayGroupSettings>("AudioPlayGroupSettings");
                    if (instance == null)
                    {
                        Debug.LogWarning("[AudioPlayGroupSettings] Not found in Resources folder. Please create one via: Create -> Audio -> Audio Play Group Settings");
                    }
                }
                return instance;
            }
        }

        [TitleGroup("播放组列表")]
        [InfoBox("所有播放组配置集中管理在此文件中。音效条目通过 playGroupName 字段引用播放组。")]
        [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowItemCount = true)]
        [SerializeField]
        private List<AudioPlayGroupEntry> playGroups = new List<AudioPlayGroupEntry>();

        public IReadOnlyList<AudioPlayGroupEntry> PlayGroups => playGroups;

        public int Count => playGroups.Count;

        public AudioPlayGroupEntry GetGroupByIndex(int index)
        {
            if (index < 0 || index >= playGroups.Count)
            {
                return null;
            }
            return playGroups[index];
        }

        public AudioPlayGroupEntry GetGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return null;
            }

            foreach (var group in playGroups)
            {
                if (group != null && group.groupName == groupName)
                {
                    return group;
                }
            }
            return null;
        }

        public bool HasGroup(string groupName)
        {
            return GetGroup(groupName) != null;
        }

        public string[] GetGroupNames()
        {
            var names = new List<string> { "" };
            foreach (var group in playGroups)
            {
                if (group != null && !string.IsNullOrEmpty(group.groupName))
                {
                    names.Add(group.groupName);
                }
            }
            return names.ToArray();
        }

        public void ResetAllSequences()
        {
            foreach (var group in playGroups)
            {
                group?.ResetSequence();
            }
        }

#if UNITY_EDITOR
        [TitleGroup("播放组列表")]
        [Button("添加播放组")]
        [GUIColor(0.4f, 0.8f, 0.4f)]
        private void AddGroup()
        {
            playGroups.Add(new AudioPlayGroupEntry { groupName = "NewGroup_" + playGroups.Count });
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [TitleGroup("统计信息")]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("播放组总数")]
        private int GroupCount => playGroups.Count;

        private void OnValidate()
        {
            var nameSet = new HashSet<string>();
            foreach (var group in playGroups)
            {
                if (group != null && !string.IsNullOrEmpty(group.groupName))
                {
                    if (nameSet.Contains(group.groupName))
                    {
                        Debug.LogWarning($"[AudioPlayGroupSettings] Duplicate group name detected: {group.groupName}");
                    }
                    else
                    {
                        nameSet.Add(group.groupName);
                    }
                }
            }
        }
#endif
    }
}
