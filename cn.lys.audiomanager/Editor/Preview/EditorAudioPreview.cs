using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Lys.Audio.Editor
{
    public static class EditorAudioPreview
    {
        private static AudioClip currentClip;
        private static float startTime;
        private static bool isPaused;
        private static float pauseTime;

        private static Type audioUtilType;
        private static MethodInfo playClipMethod;
        private static MethodInfo stopAllClipsMethod;
        private static MethodInfo pauseClipMethod;
        private static MethodInfo resumeClipMethod;
        private static MethodInfo isClipPlayingMethod;
        private static MethodInfo getClipPositionMethod;
        private static MethodInfo setClipSamplePositionMethod;

        public static bool IsPlaying
        {
            get
            {
                if (currentClip == null) return false;
                return IsClipPlayingInternal(currentClip);
            }
        }

        public static bool IsPaused => isPaused && currentClip != null;

        public static AudioClip CurrentClip => currentClip;

        public static float CurrentTime
        {
            get
            {
                if (currentClip == null) return 0f;
                if (isPaused) return pauseTime;
                return GetClipPositionInternal(currentClip);
            }
        }

        public static float Duration => currentClip != null ? currentClip.length : 0f;

        public static float Progress
        {
            get
            {
                if (currentClip == null || currentClip.length <= 0) return 0f;
                return CurrentTime / currentClip.length;
            }
        }

        static EditorAudioPreview()
        {
            InitializeAudioUtil();
        }

        private static void InitializeAudioUtil()
        {
            try
            {
                Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
                audioUtilType = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

                if (audioUtilType == null)
                {
                    Debug.LogError("[EditorAudioPreview] Failed to find AudioUtil type");
                    return;
                }

                playClipMethod = audioUtilType.GetMethod("PlayPreviewClip",
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                    null);

                stopAllClipsMethod = audioUtilType.GetMethod("StopAllPreviewClips",
                    BindingFlags.Static | BindingFlags.Public);

                pauseClipMethod = audioUtilType.GetMethod("PausePreviewClip",
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    new Type[] { typeof(AudioClip) },
                    null);

                resumeClipMethod = audioUtilType.GetMethod("ResumePreviewClip",
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    new Type[] { typeof(AudioClip) },
                    null);

                isClipPlayingMethod = audioUtilType.GetMethod("IsPreviewClipPlaying",
                    BindingFlags.Static | BindingFlags.Public);

                getClipPositionMethod = audioUtilType.GetMethod("GetPreviewClipPosition",
                    BindingFlags.Static | BindingFlags.Public);

                setClipSamplePositionMethod = audioUtilType.GetMethod("SetPreviewClipSamplePosition",
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    new Type[] { typeof(AudioClip), typeof(int) },
                    null);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EditorAudioPreview] Failed to initialize AudioUtil: {e.Message}");
            }
        }

        public static void Play(AudioClip clip, bool loop = false)
        {
            if (clip == null)
            {
                Debug.LogWarning("[EditorAudioPreview] Cannot play null clip");
                return;
            }

            Stop();

            currentClip = clip;
            isPaused = false;
            startTime = (float)EditorApplication.timeSinceStartup;

            PlayClipInternal(clip, 0, loop);
        }

        public static void PlayWithParameters(AudioClip clip, AudioClipParameters parameters)
        {
            if (clip == null) return;

            bool loop = parameters?.loop ?? false;
            Play(clip, loop);
        }

        public static void Stop()
        {
            StopAllClipsInternal();
            currentClip = null;
            isPaused = false;
            pauseTime = 0f;
        }

        public static void Pause()
        {
            if (currentClip == null || isPaused) return;

            pauseTime = GetClipPositionInternal(currentClip);
            PauseClipInternal(currentClip);
            isPaused = true;
        }

        public static void Resume()
        {
            if (currentClip == null || !isPaused) return;

            ResumeClipInternal(currentClip);
            isPaused = false;
        }

        public static void TogglePlayPause()
        {
            if (currentClip == null) return;

            if (isPaused)
            {
                Resume();
            }
            else if (IsPlaying)
            {
                Pause();
            }
        }

        public static void SetTime(float time)
        {
            if (currentClip == null) return;

            time = Mathf.Clamp(time, 0f, currentClip.length);
            int samplePosition = Mathf.RoundToInt(time * currentClip.frequency);
            SetClipSamplePositionInternal(currentClip, samplePosition);

            if (isPaused)
            {
                pauseTime = time;
            }
        }

        public static void SetProgress(float progress)
        {
            if (currentClip == null) return;

            progress = Mathf.Clamp01(progress);
            SetTime(progress * currentClip.length);
        }

        #region Internal Methods

        private static void PlayClipInternal(AudioClip clip, int startSample, bool loop)
        {
            if (playClipMethod != null)
            {
                playClipMethod.Invoke(null, new object[] { clip, startSample, loop });
            }
        }

        private static void StopAllClipsInternal()
        {
            if (stopAllClipsMethod != null)
            {
                stopAllClipsMethod.Invoke(null, null);
            }
        }

        private static void PauseClipInternal(AudioClip clip)
        {
            if (pauseClipMethod != null)
            {
                pauseClipMethod.Invoke(null, new object[] { clip });
            }
        }

        private static void ResumeClipInternal(AudioClip clip)
        {
            if (resumeClipMethod != null)
            {
                resumeClipMethod.Invoke(null, new object[] { clip });
            }
        }

        private static bool IsClipPlayingInternal(AudioClip clip)
        {
            if (isClipPlayingMethod != null)
            {
                return (bool)isClipPlayingMethod.Invoke(null, null);
            }
            return false;
        }

        private static float GetClipPositionInternal(AudioClip clip)
        {
            if (getClipPositionMethod != null)
            {
                return (float)getClipPositionMethod.Invoke(null, null);
            }
            return 0f;
        }

        private static void SetClipSamplePositionInternal(AudioClip clip, int samplePosition)
        {
            if (setClipSamplePositionMethod != null)
            {
                setClipSamplePositionMethod.Invoke(null, new object[] { clip, samplePosition });
            }
        }

        #endregion
    }
}
