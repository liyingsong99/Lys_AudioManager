using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Lys.Audio.Editor
{
    public class AudioClipParametersDrawer : OdinValueDrawer<AudioClipParameters>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var value = ValueEntry.SmartValue;

            if (label != null)
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            }

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("基础参数", EditorStyles.miniBoldLabel);
            value.volume = EditorGUILayout.Slider("音量", value.volume, 0f, 1f);
            value.pitch = EditorGUILayout.Slider("音调", value.pitch, 0.1f, 3f);
            value.loop = EditorGUILayout.Toggle("循环", value.loop);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("淡入淡出", EditorStyles.miniBoldLabel);
            value.fadeInTime = Mathf.Max(0, EditorGUILayout.FloatField("淡入时间", value.fadeInTime));
            value.fadeOutTime = Mathf.Max(0, EditorGUILayout.FloatField("淡出时间", value.fadeOutTime));

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("3D 音效", EditorStyles.miniBoldLabel);
            value.spatialBlend = EditorGUILayout.Slider("空间混合", value.spatialBlend, 0f, 1f);

            if (value.spatialBlend > 0)
            {
                EditorGUI.indentLevel++;
                value.minDistance = Mathf.Max(0, EditorGUILayout.FloatField("最小距离", value.minDistance));
                value.maxDistance = Mathf.Max(value.minDistance, EditorGUILayout.FloatField("最大距离", value.maxDistance));
                value.dopplerLevel = EditorGUILayout.Slider("多普勒效果", value.dopplerLevel, 0f, 5f);
                value.spread = EditorGUILayout.Slider("扩散", value.spread, 0f, 360f);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("其他", EditorStyles.miniBoldLabel);
            value.priority = EditorGUILayout.IntSlider("优先级", value.priority, 0, 256);
            value.ignoreListenerPause = EditorGUILayout.Toggle("忽略监听器暂停", value.ignoreListenerPause);

            if (EditorGUI.EndChangeCheck())
            {
                ValueEntry.SmartValue = value;
            }

            EditorGUI.indentLevel--;
        }
    }

    public class AudioClipEntryDrawer : OdinValueDrawer<AudioClipEntry>
    {
        private bool foldout = true;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var value = ValueEntry.SmartValue;
            if (value == null)
            {
                EditorGUILayout.LabelField("(Null Entry)");
                return;
            }

            EditorGUILayout.BeginHorizontal();
            {
                foldout = EditorGUILayout.Foldout(foldout, value.clipName ?? "(Unnamed)", true);

                if (value.editorPreviewClip != null)
                {
                    bool isPlaying = EditorAudioPreview.IsPlaying && EditorAudioPreview.CurrentClip == value.editorPreviewClip;
                    GUI.backgroundColor = isPlaying ? new Color(0.8f, 0.4f, 0.4f) : new Color(0.4f, 0.8f, 0.4f);
                    if (GUILayout.Button(isPlaying ? "■" : "▶", GUILayout.Width(25)))
                    {
                        if (isPlaying)
                        {
                            EditorAudioPreview.Stop();
                        }
                        else
                        {
                            EditorAudioPreview.Play(value.editorPreviewClip);
                        }
                    }
                    GUI.backgroundColor = Color.white;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!foldout) return;

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();

            value.clipName = EditorGUILayout.TextField("名称", value.clipName);
            value.assetPath = EditorGUILayout.TextField("资源路径", value.assetPath);
            value.eventName = EditorGUILayout.TextField("音效事件名", value.eventName);

            value.editorPreviewClip = (AudioClip)EditorGUILayout.ObjectField(
                "预览 Clip",
                value.editorPreviewClip,
                typeof(AudioClip),
                false
            );

            EditorGUILayout.Space(5);

            value.useCustomParameters = EditorGUILayout.Toggle("使用自定义参数", value.useCustomParameters);

            if (value.useCustomParameters)
            {
                if (value.customParameters == null)
                {
                    value.customParameters = new AudioClipParameters();
                }

                EditorGUI.indentLevel++;
                value.customParameters.volume = EditorGUILayout.Slider("音量", value.customParameters.volume, 0f, 1f);
                value.customParameters.pitch = EditorGUILayout.Slider("音调", value.customParameters.pitch, 0.1f, 3f);
                value.customParameters.loop = EditorGUILayout.Toggle("循环", value.customParameters.loop);
                value.customParameters.fadeInTime = EditorGUILayout.FloatField("淡入时间", value.customParameters.fadeInTime);
                value.customParameters.fadeOutTime = EditorGUILayout.FloatField("淡出时间", value.customParameters.fadeOutTime);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);

            int conditionCount = value.playConditions?.Length ?? 0;
            EditorGUILayout.LabelField($"播放条件 ({conditionCount})", EditorStyles.boldLabel);

            if (conditionCount > 0)
            {
                value.conditionOperator = (AudioConditionOperator)EditorGUILayout.EnumPopup("条件关系", value.conditionOperator);

                foreach (var condition in value.playConditions)
                {
                    if (condition != null)
                    {
                        EditorGUILayout.HelpBox($"{condition.ConditionName}: {condition.Description}", MessageType.None);
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                ValueEntry.SmartValue = value;
            }

            EditorGUI.indentLevel--;
        }
    }
}
