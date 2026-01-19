using System;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// 活动的音效实例
    /// </summary>
    public class ActiveAudioInstance
    {
        /// <summary>
        /// 唯一 ID
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 音效名称
        /// </summary>
        public string ClipName { get; private set; }

        /// <summary>
        /// AudioSource
        /// </summary>
        public AudioSource Source { get; private set; }

        /// <summary>
        /// AudioClip
        /// </summary>
        public AudioClip Clip { get; private set; }

        /// <summary>
        /// 音效参数
        /// </summary>
        public AudioClipParameters Parameters { get; private set; }

        /// <summary>
        /// 跟随目标
        /// </summary>
        public Transform FollowTarget { get; set; }

        /// <summary>
        /// 播放完成回调
        /// </summary>
        public Action OnComplete { get; set; }

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying => Source != null && Source.isPlaying;

        /// <summary>
        /// 是否已暂停
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// 是否正在淡入
        /// </summary>
        public bool IsFadingIn { get; private set; }

        /// <summary>
        /// 是否正在淡出
        /// </summary>
        public bool IsFadingOut { get; private set; }

        /// <summary>
        /// 开始播放的时间
        /// </summary>
        public float StartTime { get; private set; }

        /// <summary>
        /// 当前播放时间
        /// </summary>
        public float CurrentTime => Source != null ? Source.time : 0f;

        /// <summary>
        /// 音频总时长
        /// </summary>
        public float Duration => Clip != null ? Clip.length : 0f;

        /// <summary>
        /// 播放进度 (0-1)
        /// </summary>
        public float Progress
        {
            get
            {
                if (Clip == null || Clip.length <= 0) return 0f;
                return Source.time / Clip.length;
            }
        }

        /// <summary>
        /// 剩余时间
        /// </summary>
        public float RemainingTime
        {
            get
            {
                if (Clip == null) return 0f;
                if (Parameters.loop) return float.PositiveInfinity;
                return (Clip.length - Source.time) / Source.pitch;
            }
        }

        private float fadeStartTime;
        private float fadeDuration;
        private float fadeStartVolume;
        private float fadeTargetVolume;
        private bool stopAfterFade;

        private static int nextId = 0;

        public ActiveAudioInstance(string clipName, AudioSource source, AudioClip clip, AudioClipParameters parameters)
        {
            Id = ++nextId;
            ClipName = clipName;
            Source = source;
            Clip = clip;
            Parameters = parameters.Clone();
        }

        /// <summary>
        /// 开始播放
        /// </summary>
        public void Play()
        {
            if (Source == null || Clip == null) return;

            Source.clip = Clip;
            Parameters.ApplyTo(Source);

            if (Parameters.fadeInTime > 0)
            {
                Source.volume = 0f;
                StartFade(0f, Parameters.volume, Parameters.fadeInTime, false);
            }

            Source.Play();
            StartTime = Time.time;
            IsPaused = false;
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        /// <param name="fadeOut">是否淡出</param>
        public void Stop(bool fadeOut = true)
        {
            if (Source == null) return;

            if (fadeOut && Parameters.fadeOutTime > 0 && !IsFadingOut)
            {
                StartFade(Source.volume, 0f, Parameters.fadeOutTime, true);
            }
            else
            {
                StopImmediate();
            }
        }

        /// <summary>
        /// 停止播放（指定淡出时间）
        /// </summary>
        /// <param name="fadeOutDuration">淡出时间（秒），0 表示立即停止</param>
        public void Stop(float fadeOutDuration)
        {
            if (Source == null) return;

            if (fadeOutDuration > 0 && !IsFadingOut)
            {
                StartFade(Source.volume, 0f, fadeOutDuration, true);
            }
            else
            {
                StopImmediate();
            }
        }

        /// <summary>
        /// 立即停止
        /// </summary>
        public void StopImmediate()
        {
            if (Source != null)
            {
                Source.Stop();
                Source.clip = null;
            }

            IsFadingIn = false;
            IsFadingOut = false;
            IsPaused = false;

            OnComplete?.Invoke();
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            if (Source != null && Source.isPlaying)
            {
                Source.Pause();
                IsPaused = true;
            }
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public void Resume()
        {
            if (Source != null && IsPaused)
            {
                Source.UnPause();
                IsPaused = false;
            }
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        public void SetVolume(float volume)
        {
            Parameters.volume = volume;
            if (Source != null && !IsFadingIn && !IsFadingOut)
            {
                Source.volume = volume;
            }
        }

        /// <summary>
        /// 设置音调
        /// </summary>
        public void SetPitch(float pitch)
        {
            Parameters.pitch = pitch;
            if (Source != null)
            {
                Source.pitch = pitch;
            }
        }

        /// <summary>
        /// 更新（每帧调用）
        /// </summary>
        public void Update()
        {
            if (Source == null) return;

            if (FollowTarget != null)
            {
                Source.transform.position = FollowTarget.position;
            }

            UpdateFade();

            if (!Parameters.loop && !IsPaused && !Source.isPlaying && !IsFadingOut)
            {
                StopImmediate();
            }
        }

        private void StartFade(float from, float to, float duration, bool stopAfter)
        {
            fadeStartTime = Time.time;
            fadeDuration = duration;
            fadeStartVolume = from;
            fadeTargetVolume = to;
            stopAfterFade = stopAfter;

            IsFadingIn = to > from;
            IsFadingOut = to < from;
        }

        private void UpdateFade()
        {
            if (!IsFadingIn && !IsFadingOut) return;

            float elapsed = Time.time - fadeStartTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            float volume = Mathf.Lerp(fadeStartVolume, fadeTargetVolume, t);
            Source.volume = volume;

            if (t >= 1f)
            {
                IsFadingIn = false;
                IsFadingOut = false;

                if (stopAfterFade)
                {
                    StopImmediate();
                }
            }
        }
    }
}
