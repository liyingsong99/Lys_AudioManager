using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Lys.Audio.Editor
{
    [CustomEditor(typeof(AudioGroup))]
    public class AudioGroupInspector : OdinEditor
    {
        private AudioGroup group;
        private Vector2 bankListScroll;
        private bool showBankList = true;
        private bool showStatistics = true;

        protected override void OnEnable()
        {
            base.OnEnable();
            group = target as AudioGroup;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);

            DrawCustomActions();

            EditorGUILayout.Space(10);

            DrawStatistics();

            EditorGUILayout.Space(10);

            DrawBankPreview();
        }

        private void DrawCustomActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("快捷操作", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                {
                    GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
                    if (GUILayout.Button("打开音效编辑器", GUILayout.Height(35)))
                    {
                        AudioEditorWindow.Open(group);
                    }
                    GUI.backgroundColor = Color.white;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUI.backgroundColor = new Color(0.4f, 0.6f, 0.8f);
                    if (GUILayout.Button("创建新 Bank", GUILayout.Height(25)))
                    {
                        CreateNewBank();
                    }

                    GUI.backgroundColor = new Color(0.8f, 0.6f, 0.4f);
                    if (GUILayout.Button("扫描所有 Bank", GUILayout.Height(25)))
                    {
                        ScanAllBanks();
                    }

                    GUI.backgroundColor = Color.white;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                EditorGUILayout.BeginHorizontal();
                {
                    GUI.backgroundColor = new Color(0.6f, 0.8f, 0.4f);
                    if (GUILayout.Button("注册到 AudioManager", GUILayout.Height(25)))
                    {
                        if (Application.isPlaying)
                        {
                            AudioManager.Instance.RegisterGroup(group);
                        }
                    }

                    GUI.backgroundColor = new Color(0.8f, 0.5f, 0.5f);
                    if (GUILayout.Button("从 AudioManager 注销", GUILayout.Height(25)))
                    {
                        if (Application.isPlaying)
                        {
                            AudioManager.Instance.UnregisterGroup(group);
                        }
                    }

                    GUI.backgroundColor = Color.white;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawStatistics()
        {
            showStatistics = EditorGUILayout.Foldout(showStatistics, "统计信息", true);

            if (!showStatistics) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                int bankCount = group.AudioBanks?.Count ?? 0;
                int clipCount = group.GetClipCount();

                EditorGUILayout.LabelField("Bank 数量", bankCount.ToString());
                EditorGUILayout.LabelField("音效总数", clipCount.ToString());

                if (group.MixerGroup != null)
                {
                    EditorGUILayout.LabelField("Mixer Group", group.MixerGroup.name);
                }
                else
                {
                    EditorGUILayout.HelpBox("未设置 AudioMixerGroup", MessageType.Warning);
                }

                var settings = group.Settings;
                if (settings != null)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("组设置", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("组音量", settings.groupVolume.ToString("F2"));
                    EditorGUILayout.LabelField("静音", settings.muted ? "是" : "否");
                    EditorGUILayout.LabelField("最大同时播放", settings.maxConcurrentSounds == 0 ? "无限制" : settings.maxConcurrentSounds.ToString());
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawBankPreview()
        {
            int bankCount = group.AudioBanks?.Count ?? 0;
            showBankList = EditorGUILayout.Foldout(showBankList, $"关联的 AudioBank ({bankCount})", true);

            if (!showBankList) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                if (bankCount == 0)
                {
                    EditorGUILayout.HelpBox("没有关联的 AudioBank", MessageType.Info);
                }
                else
                {
                    bankListScroll = EditorGUILayout.BeginScrollView(bankListScroll, GUILayout.MaxHeight(150));
                    {
                        foreach (var bank in group.AudioBanks)
                        {
                            DrawBankItem(bank);
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("拖放 AudioBank 到这里添加:", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndHorizontal();

                Rect dropArea = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
                GUI.Box(dropArea, "拖放 AudioBank", EditorStyles.helpBox);

                HandleDragAndDrop(dropArea);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawBankItem(AudioBank bank)
        {
            if (bank == null)
            {
                EditorGUILayout.LabelField("(Null)", EditorStyles.miniLabel);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(bank.BankName, GUILayout.ExpandWidth(true));

                EditorGUILayout.LabelField($"({bank.GetClipCount()} clips)", EditorStyles.miniLabel, GUILayout.Width(80));

                if (GUILayout.Button("选择", GUILayout.Width(50)))
                {
                    Selection.activeObject = bank;
                    EditorGUIUtility.PingObject(bank);
                }

                if (GUILayout.Button("编辑", GUILayout.Width(50)))
                {
                    AudioEditorWindow.Open(bank);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void HandleDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        return;

                    bool hasValidBank = false;
                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        if (obj is AudioBank)
                        {
                            hasValidBank = true;
                            break;
                        }
                    }

                    if (hasValidBank)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            foreach (var obj in DragAndDrop.objectReferences)
                            {
                                if (obj is AudioBank bank)
                                {
                                    group.AddBank(bank);
                                }
                            }

                            EditorUtility.SetDirty(group);
                        }
                    }

                    evt.Use();
                    break;
            }
        }

        private void CreateNewBank()
        {
            string groupPath = AssetDatabase.GetAssetPath(group);
            string directory = System.IO.Path.GetDirectoryName(groupPath);

            string bankPath = EditorUtility.SaveFilePanelInProject(
                "创建新 AudioBank",
                "NewAudioBank",
                "asset",
                "选择保存位置",
                directory
            );

            if (string.IsNullOrEmpty(bankPath))
                return;

            var newBank = ScriptableObject.CreateInstance<AudioBank>();
            newBank.BankName = System.IO.Path.GetFileNameWithoutExtension(bankPath);

            AssetDatabase.CreateAsset(newBank, bankPath);
            AssetDatabase.SaveAssets();

            group.AddBank(newBank);
            EditorUtility.SetDirty(group);

            Selection.activeObject = newBank;
            EditorGUIUtility.PingObject(newBank);
        }

        private void ScanAllBanks()
        {
            foreach (var bank in group.AudioBanks)
            {
                if (bank != null)
                {
                    AudioFolderScanner.ScanAllFolders(bank);
                }
            }

            Debug.Log($"[AudioGroup] Scanned all banks in group: {group.GroupName}");
        }
    }
}
