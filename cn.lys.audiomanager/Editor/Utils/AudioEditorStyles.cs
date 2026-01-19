using UnityEngine;
using UnityEditor;

namespace Lys.Audio.Editor
{
    public static class AudioEditorStyles
    {
        private static GUIStyle toolbarButton;
        private static GUIStyle panelHeader;
        private static GUIStyle listItem;
        private static GUIStyle listItemSelected;
        private static GUIStyle conditionBox;
        private static GUIStyle parameterLabel;
        private static GUIStyle waveformBackground;
        private static GUIStyle statusLabel;

        public static GUIStyle ToolbarButton
        {
            get
            {
                if (toolbarButton == null)
                {
                    toolbarButton = new GUIStyle(EditorStyles.toolbarButton)
                    {
                        fontSize = 12,
                        fontStyle = FontStyle.Normal,
                        fixedHeight = 24
                    };
                }
                return toolbarButton;
            }
        }

        public static GUIStyle PanelHeader
        {
            get
            {
                if (panelHeader == null)
                {
                    panelHeader = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 13,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset(8, 8, 4, 4)
                    };
                }
                return panelHeader;
            }
        }

        public static GUIStyle ListItem
        {
            get
            {
                if (listItem == null)
                {
                    listItem = new GUIStyle(EditorStyles.label)
                    {
                        padding = new RectOffset(8, 8, 4, 4),
                        margin = new RectOffset(0, 0, 1, 1)
                    };
                    listItem.normal.background = CreateColorTexture(new Color(0.22f, 0.22f, 0.22f, 0.5f));
                }
                return listItem;
            }
        }

        public static GUIStyle ListItemSelected
        {
            get
            {
                if (listItemSelected == null)
                {
                    listItemSelected = new GUIStyle(ListItem);
                    listItemSelected.normal.background = CreateColorTexture(new Color(0.17f, 0.36f, 0.53f, 1f));
                    listItemSelected.normal.textColor = Color.white;
                }
                return listItemSelected;
            }
        }

        public static GUIStyle ConditionBox
        {
            get
            {
                if (conditionBox == null)
                {
                    conditionBox = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(8, 8, 8, 8),
                        margin = new RectOffset(4, 4, 4, 4)
                    };
                }
                return conditionBox;
            }
        }

        public static GUIStyle ParameterLabel
        {
            get
            {
                if (parameterLabel == null)
                {
                    parameterLabel = new GUIStyle(EditorStyles.label)
                    {
                        fontStyle = FontStyle.Bold,
                        fontSize = 11
                    };
                }
                return parameterLabel;
            }
        }

        public static GUIStyle WaveformBackground
        {
            get
            {
                if (waveformBackground == null)
                {
                    waveformBackground = new GUIStyle();
                    waveformBackground.normal.background = CreateColorTexture(new Color(0.15f, 0.15f, 0.15f, 1f));
                }
                return waveformBackground;
            }
        }

        public static GUIStyle StatusLabel
        {
            get
            {
                if (statusLabel == null)
                {
                    statusLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                    {
                        fontSize = 10
                    };
                }
                return statusLabel;
            }
        }

        private static Texture2D CreateColorTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        public static class Colors
        {
            public static Color PlayButton = new Color(0.4f, 0.8f, 0.4f);
            public static Color StopButton = new Color(0.8f, 0.4f, 0.4f);
            public static Color RefreshButton = new Color(0.4f, 0.6f, 0.8f);
            public static Color SaveButton = new Color(0.8f, 0.6f, 0.4f);
            public static Color AddButton = new Color(0.4f, 0.7f, 0.4f);
            public static Color RemoveButton = new Color(0.7f, 0.3f, 0.3f);
            public static Color WaveformColor = new Color(0.3f, 0.7f, 0.3f, 0.8f);
            public static Color ProgressColor = new Color(0.5f, 0.5f, 1f, 0.5f);
        }

        public static class Icons
        {
            public const string Play = "d_PlayButton";
            public const string Stop = "d_PreMatQuad";
            public const string Pause = "d_PauseButton";
            public const string Refresh = "d_Refresh";
            public const string Save = "d_SaveActive";
            public const string Add = "d_Toolbar Plus";
            public const string Remove = "d_Toolbar Minus";
            public const string Settings = "d_Settings";
            public const string Search = "d_Search Icon";
            public const string Folder = "d_Folder Icon";
            public const string Audio = "d_AudioClip Icon";
        }

        public static void ResetStyles()
        {
            toolbarButton = null;
            panelHeader = null;
            listItem = null;
            listItemSelected = null;
            conditionBox = null;
            parameterLabel = null;
            waveformBackground = null;
            statusLabel = null;
        }
    }
}
