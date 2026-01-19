using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Lys.Audio.Editor
{
    public class AudioEditorWindow : OdinEditorWindow
    {
        private static AudioEditorWindow instance;

        [MenuItem("Tools/Audio/Audio Editor")]
        private static void OpenWindow()
        {
            instance = GetWindow<AudioEditorWindow>();
            instance.titleContent = new GUIContent("Audio Editor", EditorGUIUtility.IconContent("d_AudioClip Icon").image);
            instance.minSize = new Vector2(900, 600);
            instance.Show();
        }

        public static void Open(AudioGroup group)
        {
            OpenWindow();
            if (instance != null && group != null)
            {
                instance.SelectGroup(group);
            }
        }

        public static void Open(AudioBank bank)
        {
            OpenWindow();
            if (instance != null && bank != null)
            {
                instance.SelectBank(bank);
            }
        }

        [HideInInspector]
        [SerializeField]
        private string searchFilter = "";

        private List<AudioGroup> allGroups = new List<AudioGroup>();
        private List<AudioBank> allBanks = new List<AudioBank>();
        private List<AudioClipEntry> filteredClips = new List<AudioClipEntry>();

        [HideInInspector]
        [SerializeField]
        private AudioGroup selectedGroup;

        [HideInInspector]
        [SerializeField]
        private AudioBank selectedBank;

        [HideInInspector]
        [SerializeField]
        private AudioClipEntry selectedEntry;

        private Vector2 leftPanelScroll;
        private Vector2 centerPanelScroll;
        private Vector2 rightPanelScroll;

        protected override void OnEnable()
        {
            base.OnEnable();
            LoadAllBanksAndGroups();
            EditorApplication.update += OnEditorUpdate;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EditorAudioPreview.Stop();
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (EditorAudioPreview.IsPlaying)
            {
                Repaint();
            }
        }

        protected override void OnImGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            {
                DrawLeftPanel();
                DrawVerticalSeparator();
                DrawCenterPanel();
                DrawVerticalSeparator();
                DrawRightPanel();
            }
            EditorGUILayout.EndHorizontal();

            DrawStatusBar();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUI.backgroundColor = new Color(0.4f, 0.6f, 0.8f);
                if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    LoadAllBanksAndGroups();
                    Repaint();
                }

                GUI.backgroundColor = new Color(0.8f, 0.6f, 0.4f);
                if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    AssetDatabase.SaveAssets();
                    Debug.Log("[AudioEditor] All changes saved");
                }

                GUI.backgroundColor = Color.white;

                GUILayout.FlexibleSpace();

                GUILayout.Label("搜索:", GUILayout.Width(40));
                EditorGUI.BeginChangeCheck();
                searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(200));
                if (EditorGUI.EndChangeCheck())
                {
                    FilterClips();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            {
                EditorGUILayout.LabelField("音效列表", AudioEditorStyles.PanelHeader);
                EditorGUILayout.Space(5);

                EditorGUI.BeginChangeCheck();
                int groupIndex = allGroups.IndexOf(selectedGroup);
                string[] groupNames = allGroups.Select(g => g != null ? g.GroupName : "(None)").Prepend("全部").ToArray();
                int newGroupIndex = EditorGUILayout.Popup("AudioGroup", groupIndex + 1, groupNames) - 1;
                if (EditorGUI.EndChangeCheck())
                {
                    selectedGroup = newGroupIndex >= 0 ? allGroups[newGroupIndex] : null;
                    selectedEntry = null;
                    FilterClips();
                }

                EditorGUILayout.Space(10);

                leftPanelScroll = EditorGUILayout.BeginScrollView(leftPanelScroll, GUILayout.ExpandHeight(true));
                {
                    if (filteredClips.Count == 0)
                    {
                        EditorGUILayout.HelpBox("没有找到音效", MessageType.Info);
                    }
                    else
                    {
                        foreach (var entry in filteredClips)
                        {
                            DrawClipListItem(entry);
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawClipListItem(AudioClipEntry entry)
        {
            if (entry == null) return;

            bool isSelected = selectedEntry == entry;
            bool isPlaying = EditorAudioPreview.IsPlaying && EditorAudioPreview.CurrentClip == entry.editorPreviewClip;

            GUIStyle style = isSelected ? AudioEditorStyles.ListItemSelected : AudioEditorStyles.ListItem;

            Rect rowRect = EditorGUILayout.BeginHorizontal(style);

            if (isSelected && Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rowRect, new Color(0.17f, 0.36f, 0.53f, 1f));
            }

            {
                string statusIcon = isPlaying ? "►" : "○";
                GUILayout.Label(statusIcon, GUILayout.Width(16));

                if (GUILayout.Button(entry.clipName, EditorStyles.label, GUILayout.ExpandWidth(true)))
                {
                    SelectEntry(entry);
                }

                GUI.backgroundColor = isPlaying ? new Color(0.8f, 0.4f, 0.4f) : new Color(0.4f, 0.8f, 0.4f);
                if (GUILayout.Button(isPlaying ? "■" : "▶", GUILayout.Width(24), GUILayout.Height(18)))
                {
                    if (isPlaying)
                    {
                        EditorAudioPreview.Stop();
                    }
                    else if (entry.editorPreviewClip != null)
                    {
                        EditorAudioPreview.Play(entry.editorPreviewClip);
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCenterPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            {
                EditorGUILayout.LabelField("播放条件", AudioEditorStyles.PanelHeader);
                EditorGUILayout.Space(5);

                centerPanelScroll = EditorGUILayout.BeginScrollView(centerPanelScroll);
                {
                    if (selectedEntry != null)
                    {
                        DrawConditionsEditor();
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("请选择一个音效", MessageType.Info);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawConditionsEditor()
        {
            EditorGUILayout.LabelField("播放条件", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            selectedEntry.conditionOperator = (AudioConditionOperator)EditorGUILayout.EnumPopup("条件关系", selectedEntry.conditionOperator);
            if (EditorGUI.EndChangeCheck())
            {
                MarkDirty();
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+ 添加条件", GUILayout.Width(100)))
                {
                    ShowAddConditionMenu();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (selectedEntry.playConditions != null)
            {
                for (int i = 0; i < selectedEntry.playConditions.Length; i++)
                {
                    var condition = selectedEntry.playConditions[i];
                    if (condition == null) continue;

                    EditorGUILayout.BeginVertical(AudioEditorStyles.ConditionBox);
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField(condition.ConditionName, EditorStyles.boldLabel);
                            GUILayout.FlexibleSpace();
                            GUI.color = AudioEditorStyles.Colors.RemoveButton;
                            if (GUILayout.Button("×", GUILayout.Width(20)))
                            {
                                RemoveCondition(i);
                            }
                            GUI.color = Color.white;
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.HelpBox(condition.Description, MessageType.None);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            {
                EditorGUILayout.LabelField("音效参数", AudioEditorStyles.PanelHeader);
                EditorGUILayout.Space(5);

                rightPanelScroll = EditorGUILayout.BeginScrollView(rightPanelScroll);
                {
                    if (selectedEntry != null)
                    {
                        DrawParametersEditor();
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("请选择一个音效", MessageType.Info);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawParametersEditor()
        {
            EditorGUILayout.LabelField("基础信息", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("名称", selectedEntry.clipName);
            EditorGUILayout.TextField("路径", selectedEntry.assetPath);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            selectedEntry.eventName = EditorGUILayout.TextField("音效事件名", selectedEntry.eventName);
            if (EditorGUI.EndChangeCheck())
            {
                MarkDirty();
            }

            EditorGUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            selectedEntry.useCustomParameters = EditorGUILayout.Toggle("使用自定义参数", selectedEntry.useCustomParameters);
            if (EditorGUI.EndChangeCheck())
            {
                if (selectedEntry.useCustomParameters && selectedEntry.customParameters == null)
                {
                    selectedEntry.customParameters = new AudioClipParameters();
                }
                MarkDirty();
            }

            if (selectedEntry.useCustomParameters && selectedEntry.customParameters != null)
            {
                EditorGUILayout.Space(5);
                DrawAudioClipParameters(selectedEntry.customParameters);
            }
        }

        private void DrawAudioClipParameters(AudioClipParameters parameters)
        {
            EditorGUI.BeginChangeCheck();

            parameters.volume = EditorGUILayout.Slider("音量", parameters.volume, 0f, 1f);
            parameters.pitch = EditorGUILayout.Slider("音调", parameters.pitch, 0.1f, 3f);
            parameters.loop = EditorGUILayout.Toggle("循环", parameters.loop);
            parameters.fadeInTime = EditorGUILayout.FloatField("淡入时间", parameters.fadeInTime);
            parameters.fadeOutTime = EditorGUILayout.FloatField("淡出时间", parameters.fadeOutTime);
            parameters.spatialBlend = EditorGUILayout.Slider("空间混合", parameters.spatialBlend, 0f, 1f);

            if (parameters.spatialBlend > 0)
            {
                EditorGUI.indentLevel++;
                parameters.minDistance = EditorGUILayout.FloatField("最小距离", parameters.minDistance);
                parameters.maxDistance = EditorGUILayout.FloatField("最大距离", parameters.maxDistance);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                MarkDirty();
            }
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label($"Groups: {allGroups.Count} | Banks: {allBanks.Count} | Clips: {filteredClips.Count}", AudioEditorStyles.StatusLabel);
                GUILayout.FlexibleSpace();

                if (EditorAudioPreview.IsPlaying)
                {
                    GUILayout.Label($"正在播放: {EditorAudioPreview.CurrentClip?.name}", AudioEditorStyles.StatusLabel);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawVerticalSeparator()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(1));
            Rect rect = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f));
            EditorGUILayout.EndVertical();
        }

        private void LoadAllBanksAndGroups()
        {
            allGroups.Clear();
            allBanks.Clear();

            string[] groupGuids = AssetDatabase.FindAssets("t:AudioGroup");
            foreach (string guid in groupGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var group = AssetDatabase.LoadAssetAtPath<AudioGroup>(path);
                if (group != null)
                {
                    allGroups.Add(group);
                }
            }

            string[] bankGuids = AssetDatabase.FindAssets("t:AudioBank");
            foreach (string guid in bankGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var bank = AssetDatabase.LoadAssetAtPath<AudioBank>(path);
                if (bank != null)
                {
                    allBanks.Add(bank);

                    if (bank.FolderEntries != null && bank.FolderEntries.Count > 0)
                    {
                        AudioFolderScanner.ScanAllFolders(bank);
                    }
                }
            }

            FilterClips();
        }

        private void FilterClips()
        {
            filteredClips.Clear();

            IEnumerable<AudioClipEntry> clips;

            if (selectedGroup != null)
            {
                clips = selectedGroup.GetAllClips();
            }
            else
            {
                clips = allBanks.SelectMany(b => b.GetAllClips());
            }

            if (!string.IsNullOrEmpty(searchFilter))
            {
                string filter = searchFilter.ToLowerInvariant();
                clips = clips.Where(c => c.clipName.ToLowerInvariant().Contains(filter));
            }

            filteredClips.AddRange(clips);
        }

        private void SelectGroup(AudioGroup group)
        {
            selectedGroup = group;
            selectedBank = null;
            selectedEntry = null;
            FilterClips();
        }

        private void SelectBank(AudioBank bank)
        {
            selectedBank = bank;
            selectedEntry = null;

            selectedGroup = allGroups.FirstOrDefault(g => ContainsBank(g, bank));

            FilterClips();
        }

        private bool ContainsBank(AudioGroup group, AudioBank bank)
        {
            if (group == null || bank == null) return false;
            foreach (var b in group.AudioBanks)
            {
                if (b == bank) return true;
            }
            return false;
        }

        private void SelectEntry(AudioClipEntry entry)
        {
            selectedEntry = entry;

            if (selectedBank == null && entry != null)
            {
                selectedBank = FindBankContainingEntry(entry);
            }

            Repaint();
        }

        private AudioBank FindBankContainingEntry(AudioClipEntry entry)
        {
            if (entry == null) return null;

            foreach (var bank in allBanks)
            {
                if (bank == null) continue;
                foreach (var clip in bank.GetAllClips())
                {
                    if (clip == entry)
                    {
                        return bank;
                    }
                }
            }
            return null;
        }

        private void ShowAddConditionMenu()
        {
            GenericMenu menu = new GenericMenu();

            var conditionInfo = AudioConditionFactory.GetAllConditionInfo();
            foreach (var kvp in conditionInfo)
            {
                string conditionId = kvp.Key;
                string displayName = kvp.Value;
                menu.AddItem(new GUIContent(displayName), false, () => AddCondition(conditionId));
            }

            menu.ShowAsContext();
        }

        private void AddCondition(string conditionId)
        {
            if (selectedEntry == null) return;

            var condition = AudioConditionFactory.CreateCondition(conditionId);
            if (condition == null) return;

            var conditions = selectedEntry.playConditions?.ToList() ?? new List<IAudioPlayCondition>();
            conditions.Add(condition);
            selectedEntry.playConditions = conditions.ToArray();

            MarkDirty();
        }

        private void RemoveCondition(int index)
        {
            if (selectedEntry?.playConditions == null) return;
            if (index < 0 || index >= selectedEntry.playConditions.Length) return;

            var conditions = selectedEntry.playConditions.ToList();
            conditions.RemoveAt(index);
            selectedEntry.playConditions = conditions.ToArray();

            MarkDirty();
        }

        private void MarkDirty()
        {
            if (selectedBank != null)
            {
                EditorUtility.SetDirty(selectedBank);
            }
        }
    }
}
