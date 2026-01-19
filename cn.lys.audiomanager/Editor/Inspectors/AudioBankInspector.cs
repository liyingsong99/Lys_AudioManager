using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Lys.Audio.Editor
{
    [CustomEditor(typeof(AudioBank))]
    public class AudioBankInspector : OdinEditor
    {
        private AudioBank bank;
        private Vector2 clipListScroll;
        private bool showClipList = true;

        protected override void OnEnable()
        {
            base.OnEnable();
            bank = target as AudioBank;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);

            DrawCustomActions();

            EditorGUILayout.Space(10);

            DrawClipPreviewSection();
        }

        private void DrawCustomActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("快捷操作", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                {
                    GUI.backgroundColor = new Color(0.4f, 0.6f, 0.8f);
                    if (GUILayout.Button("在编辑器中打开", GUILayout.Height(30)))
                    {
                        AudioEditorWindow.Open(bank);
                    }

                    GUI.backgroundColor = new Color(0.8f, 0.6f, 0.4f);
                    if (GUILayout.Button("扫描所有文件夹", GUILayout.Height(30)))
                    {
                        AudioFolderScanner.ScanAllFolders(bank);
                    }

                    GUI.backgroundColor = Color.white;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
                    if (GUILayout.Button("验证所有路径", GUILayout.Height(25)))
                    {
                        ValidateAllPaths();
                    }

                    GUI.backgroundColor = new Color(0.7f, 0.7f, 0.4f);
                    if (GUILayout.Button("清理无效条目", GUILayout.Height(25)))
                    {
                        CleanInvalidEntries();
                    }

                    GUI.backgroundColor = Color.white;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawClipPreviewSection()
        {
            showClipList = EditorGUILayout.Foldout(showClipList, $"音效预览 ({bank.GetClipCount()} 个)", true);

            if (!showClipList) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                if (EditorAudioPreview.IsPlaying)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField($"正在播放: {EditorAudioPreview.CurrentClip?.name}", EditorStyles.miniLabel);
                        if (GUILayout.Button("停止", GUILayout.Width(50)))
                        {
                            EditorAudioPreview.Stop();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(5);

                clipListScroll = EditorGUILayout.BeginScrollView(clipListScroll, GUILayout.MaxHeight(200));
                {
                    foreach (var entry in bank.GetAllClips())
                    {
                        DrawClipPreviewItem(entry);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawClipPreviewItem(AudioClipEntry entry)
        {
            if (entry == null) return;

            bool isPlaying = EditorAudioPreview.IsPlaying && EditorAudioPreview.CurrentClip == entry.editorPreviewClip;

            EditorGUILayout.BeginHorizontal();
            {
                string statusIcon = isPlaying ? "►" : "○";
                GUILayout.Label(statusIcon, GUILayout.Width(16));

                EditorGUILayout.LabelField(entry.clipName, GUILayout.ExpandWidth(true));

                GUI.backgroundColor = isPlaying ? new Color(0.8f, 0.4f, 0.4f) : new Color(0.4f, 0.8f, 0.4f);
                string buttonText = isPlaying ? "停止" : "播放";
                if (GUILayout.Button(buttonText, GUILayout.Width(50)))
                {
                    if (isPlaying)
                    {
                        EditorAudioPreview.Stop();
                    }
                    else if (entry.editorPreviewClip != null)
                    {
                        EditorAudioPreview.Play(entry.editorPreviewClip);
                    }
                    else
                    {
                        entry.editorPreviewClip = AssetDatabase.LoadAssetAtPath<AudioClip>(entry.assetPath);
                        if (entry.editorPreviewClip != null)
                        {
                            EditorAudioPreview.Play(entry.editorPreviewClip);
                        }
                        else
                        {
                            Debug.LogWarning($"[AudioBank] Failed to load clip: {entry.assetPath}");
                        }
                    }
                }
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("定位", GUILayout.Width(40)))
                {
                    if (entry.editorPreviewClip != null)
                    {
                        EditorGUIUtility.PingObject(entry.editorPreviewClip);
                        Selection.activeObject = entry.editorPreviewClip;
                    }
                    else
                    {
                        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(entry.assetPath);
                        if (clip != null)
                        {
                            EditorGUIUtility.PingObject(clip);
                            Selection.activeObject = clip;
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ValidateAllPaths()
        {
            int invalidCount = 0;
            foreach (var entry in bank.GetAllClips())
            {
                if (string.IsNullOrEmpty(entry.assetPath))
                {
                    Debug.LogWarning($"[AudioBank] Entry '{entry.clipName}' has empty asset path");
                    invalidCount++;
                    continue;
                }

                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(entry.assetPath);
                if (clip == null)
                {
                    Debug.LogWarning($"[AudioBank] Entry '{entry.clipName}' has invalid path: {entry.assetPath}");
                    invalidCount++;
                }
            }

            if (invalidCount == 0)
            {
                Debug.Log($"[AudioBank] All {bank.GetClipCount()} paths are valid");
            }
            else
            {
                Debug.LogWarning($"[AudioBank] Found {invalidCount} invalid paths");
            }
        }

        private void CleanInvalidEntries()
        {
            Debug.Log("[AudioBank] Clean invalid entries - This feature requires manual review");
            EditorUtility.DisplayDialog("提示", "请手动检查并删除无效的音效条目。", "确定");
        }
    }
}
