using UnityEngine;
using UnityEditor;

namespace Lys.Audio.Editor
{
    public static class RuntimeAudioPreview
    {
        private static ActiveAudioInstance currentInstance;
        private static string currentClipName;

        public static bool IsPlaying => currentInstance != null && currentInstance.IsPlaying;
        public static bool IsPaused => currentInstance != null && currentInstance.IsPaused;
        public static string CurrentClipName => currentClipName;
        public static float CurrentTime => currentInstance?.CurrentTime ?? 0f;
        public static float Duration => currentInstance?.Duration ?? 0f;

        public static float Progress
        {
            get
            {
                if (currentInstance == null || Duration <= 0) return 0f;
                return CurrentTime / Duration;
            }
        }

        public static void Play(string clipName, AudioClipParameters parameters = null)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[RuntimeAudioPreview] Runtime preview requires play mode");
                return;
            }

            if (string.IsNullOrEmpty(clipName))
            {
                Debug.LogWarning("[RuntimeAudioPreview] clipName is null or empty");
                return;
            }

            Stop();

            currentClipName = clipName;

            AudioManager.Instance.PlayAsync(clipName, (inst) =>
            {
                currentInstance = inst;
                if (currentInstance == null)
                {
                    Debug.LogWarning($"[RuntimeAudioPreview] Failed to play: {clipName}");
                    currentClipName = null;
                }
            }, null, parameters);
        }

        public static void PlayEntry(AudioClipEntry entry, AudioBank bank = null)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[RuntimeAudioPreview] Runtime preview requires play mode");
                return;
            }

            if (entry == null)
            {
                Debug.LogWarning("[RuntimeAudioPreview] entry is null");
                return;
            }

            Stop();

            currentClipName = entry.clipName;

            var parameters = entry.GetParameters(bank?.DefaultParameters);

            AudioManager.Instance.PlayAsync(entry.clipName, (inst) =>
            {
                currentInstance = inst;
                if (currentInstance == null)
                {
                    Debug.LogWarning($"[RuntimeAudioPreview] Failed to play entry: {entry.clipName}");
                    currentClipName = null;
                }
            }, null, parameters);
        }

        public static void Stop()
        {
            if (currentInstance != null)
            {
                currentInstance.Stop(false);
                currentInstance = null;
            }
            currentClipName = null;
        }

        public static void Pause()
        {
            currentInstance?.Pause();
        }

        public static void Resume()
        {
            currentInstance?.Resume();
        }

        public static void TogglePlayPause()
        {
            if (currentInstance == null) return;

            if (currentInstance.IsPaused)
            {
                Resume();
            }
            else if (currentInstance.IsPlaying)
            {
                Pause();
            }
        }

        public static void SetTime(float time)
        {
            if (currentInstance?.Source != null)
            {
                currentInstance.Source.time = Mathf.Clamp(time, 0f, Duration);
            }
        }

        public static void SetProgress(float progress)
        {
            SetTime(progress * Duration);
        }

        public static void SetVolume(float volume)
        {
            currentInstance?.SetVolume(volume);
        }

        public static void SetPitch(float pitch)
        {
            currentInstance?.SetPitch(pitch);
        }
    }
}
