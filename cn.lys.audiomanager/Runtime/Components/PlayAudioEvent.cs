using System.Collections;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// 播放音频事件
    /// 支持GameObject隐藏时暂停音效，启用时恢复播放
    /// </summary>
    public class PlayAudioEvent : MonoBehaviour
    {
        public string audioEventName;

        [Tooltip("启用时立即播放音频")]
        public bool PlayOnEnable = false;

        [Tooltip("隐藏时暂停音频（而不是停止）")]
        public bool PauseOnHide = true;

        [Tooltip("隐藏时停止音频")]
        public bool StopOnHide = false;

        private bool isPlaying = false;
        private bool wasPausedByHide = false;
        private ActiveAudioInstance currentActiveInstance = null;

        void Awake()
        {
            if (string.IsNullOrEmpty(audioEventName))
            {
                this.enabled = false;
                return;
            }
        }

        void OnEnable()
        {
            if (wasPausedByHide && currentActiveInstance != null && currentActiveInstance.Source != null)
            {
                ResumeAudio();
            }
            else if (PlayOnEnable && !string.IsNullOrEmpty(audioEventName))
            {
                Play();
            }
        }

        void OnDisable()
        {
            if (isPlaying && currentActiveInstance != null && currentActiveInstance.Source != null)
            {
                if (PauseOnHide)
                {
                    PauseAudio();
                }
                else if (StopOnHide)
                {
                    Stop();
                }
            }
        }

        public void Play()
        {
            if (isPlaying && currentActiveInstance != null && currentActiveInstance.Source != null)
            {
                return;
            }

            isPlaying = true;
            wasPausedByHide = false;

            currentActiveInstance = AudioAPI.Play(audioEventName);

            if (currentActiveInstance != null)
            {
                currentActiveInstance.OnComplete += OnAudioCompleted;
            }

            StartCoroutine(Reset());
        }

        IEnumerator Reset()
        {
            yield return new WaitForSeconds(0.1f);
        }

        private void PauseAudio()
        {
            if (currentActiveInstance != null && currentActiveInstance.IsPlaying)
            {
                currentActiveInstance.Pause();
                wasPausedByHide = true;

#if UNITY_EDITOR
                Debug.Log($"暂停音效: {audioEventName}");
#endif
            }
        }

        private void ResumeAudio()
        {
            if (currentActiveInstance != null && wasPausedByHide)
            {
                currentActiveInstance.Resume();
                wasPausedByHide = false;

#if UNITY_EDITOR
                Debug.Log($"恢复音效: {audioEventName}");
#endif
            }
        }

        public void Stop()
        {
            if (currentActiveInstance != null)
            {
                currentActiveInstance.StopImmediate();
            }
            else
            {
                AudioAPI.Stop(audioEventName, 0f);
            }

            ResetPlayingState();
        }

        private void OnAudioCompleted()
        {
            ResetPlayingState();
        }

        private void ResetPlayingState()
        {
            isPlaying = false;
            wasPausedByHide = false;

            if (currentActiveInstance != null)
            {
                currentActiveInstance.OnComplete -= OnAudioCompleted;
                currentActiveInstance = null;
            }
        }

        public bool IsCurrentlyPlaying()
        {
            return isPlaying && currentActiveInstance != null &&
                   (currentActiveInstance.IsPlaying || wasPausedByHide);
        }

        public bool IsCurrentlyPaused()
        {
            return wasPausedByHide && currentActiveInstance != null && currentActiveInstance.IsPaused;
        }

        public void ManualPause()
        {
            if (isPlaying && currentActiveInstance != null && currentActiveInstance.IsPlaying)
            {
                currentActiveInstance.Pause();
            }
        }

        public void ManualResume()
        {
            if (isPlaying && currentActiveInstance != null && currentActiveInstance.IsPaused)
            {
                currentActiveInstance.Resume();
            }
        }

        void OnDestroy()
        {
            ResetPlayingState();
        }
    }
}
