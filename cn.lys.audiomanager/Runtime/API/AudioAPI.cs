using System;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// 音效管理器对外 API 静态类
    /// 提供便捷的静态方法访问 AudioManager 功能
    /// </summary>
    public static class AudioAPI
    {
        #region Play - Sync

        /// <summary>
        /// 播放音效（同步方式，如果音效未加载则同步加载）
        /// </summary>
        /// <param name="clipName">音效名称</param>
        /// <param name="position">播放位置（可选）</param>
        /// <param name="overrideParams">覆盖参数（可选）</param>
        /// <returns>活动音效实例，失败返回 null</returns>
        public static ActiveAudioInstance Play(string clipName, Vector3? position = null, AudioClipParameters overrideParams = null)
        {
            return AudioManager.Instance.Play(clipName, position, overrideParams);
        }

        /// <summary>
        /// 播放跟随目标的音效（同步）
        /// </summary>
        /// <param name="clipName">音效名称</param>
        /// <param name="target">跟随目标</param>
        /// <param name="overrideParams">覆盖参数（可选）</param>
        /// <returns>活动音效实例，失败返回 null</returns>
        public static ActiveAudioInstance PlayFollow(string clipName, Transform target, AudioClipParameters overrideParams = null)
        {
            return AudioManager.Instance.PlayFollow(clipName, target, overrideParams);
        }

        /// <summary>
        /// 在指定位置播放音效（同步，简化接口）
        /// </summary>
        public static ActiveAudioInstance PlayAt(string clipName, Vector3 position)
        {
            return AudioManager.Instance.Play(clipName, position, null);
        }

        /// <summary>
        /// 播放 2D 音效（同步，简化接口）
        /// </summary>
        public static ActiveAudioInstance Play2D(string clipName)
        {
            return AudioManager.Instance.Play(clipName, null, null);
        }

        #endregion

        #region Play - Async (Callback)

        /// <summary>
        /// 播放音效（异步方式，回调返回结果）
        /// </summary>
        /// <param name="clipName">音效名称</param>
        /// <param name="onComplete">播放完成回调，参数为活动音效实例（失败时为 null）</param>
        /// <param name="position">播放位置（可选）</param>
        /// <param name="overrideParams">覆盖参数（可选）</param>
        public static void PlayAsync(string clipName, Action<ActiveAudioInstance> onComplete, Vector3? position = null, AudioClipParameters overrideParams = null)
        {
            AudioManager.Instance.PlayAsync(clipName, onComplete, position, overrideParams);
        }

        /// <summary>
        /// 播放跟随目标的音效（异步）
        /// </summary>
        /// <param name="clipName">音效名称</param>
        /// <param name="target">跟随目标</param>
        /// <param name="onComplete">播放完成回调</param>
        /// <param name="overrideParams">覆盖参数（可选）</param>
        public static void PlayFollowAsync(string clipName, Transform target, Action<ActiveAudioInstance> onComplete, AudioClipParameters overrideParams = null)
        {
            AudioManager.Instance.PlayFollowAsync(clipName, target, onComplete, overrideParams);
        }

        /// <summary>
        /// 在指定位置播放音效（异步，简化接口）
        /// </summary>
        public static void PlayAtAsync(string clipName, Vector3 position, Action<ActiveAudioInstance> onComplete)
        {
            AudioManager.Instance.PlayAsync(clipName, onComplete, position, null);
        }

        /// <summary>
        /// 播放 2D 音效（异步，简化接口）
        /// </summary>
        public static void Play2DAsync(string clipName, Action<ActiveAudioInstance> onComplete)
        {
            AudioManager.Instance.PlayAsync(clipName, onComplete, null, null);
        }

        #endregion

        #region Stop

        /// <summary>
        /// 停止指定音效
        /// </summary>
        /// <param name="clipName">音效名称</param>
        /// <param name="fadeOut">是否淡出</param>
        public static void Stop(string clipName, bool fadeOut = true)
        {
            AudioManager.Instance.Stop(clipName, fadeOut);
        }

        /// <summary>
        /// 停止指定音效（指定淡出时间）
        /// </summary>
        /// <param name="clipName">音效名称</param>
        /// <param name="fadeOutDuration">淡出时间（秒），0 表示立即停止</param>
        public static void Stop(string clipName, float fadeOutDuration)
        {
            AudioManager.Instance.Stop(clipName, fadeOutDuration);
        }

        /// <summary>
        /// 停止所有音效
        /// </summary>
        /// <param name="fadeOut">是否淡出</param>
        /// <param name="onlyLooping">仅停止循环音效</param>
        public static void StopAll(bool fadeOut = true, bool onlyLooping = false)
        {
            AudioManager.Instance.StopAll(fadeOut, onlyLooping);
        }

        /// <summary>
        /// 立即停止所有音效
        /// </summary>
        public static void StopAllImmediate()
        {
            AudioManager.Instance.StopAllImmediate();
        }

        /// <summary>
        /// 停止指定实例
        /// </summary>
        /// <param name="instanceId">实例 ID</param>
        /// <param name="fadeOut">是否淡出</param>
        public static void StopInstance(int instanceId, bool fadeOut = true)
        {
            AudioManager.Instance.StopInstance(instanceId, fadeOut);
        }

        /// <summary>
        /// 停止指定实例
        /// </summary>
        /// <param name="instance">音效实例</param>
        /// <param name="fadeOut">是否淡出</param>
        public static void StopInstance(ActiveAudioInstance instance, bool fadeOut = true)
        {
            if (instance != null)
            {
                instance.Stop(fadeOut);
            }
        }

        #endregion

        #region Control

        /// <summary>
        /// 暂停指定音效
        /// </summary>
        public static void Pause(string clipName)
        {
            AudioManager.Instance.Pause(clipName);
        }

        /// <summary>
        /// 恢复指定音效
        /// </summary>
        public static void Resume(string clipName)
        {
            AudioManager.Instance.Resume(clipName);
        }

        /// <summary>
        /// 暂停所有音效
        /// </summary>
        public static void PauseAll()
        {
            AudioManager.Instance.PauseAll();
        }

        /// <summary>
        /// 恢复所有音效
        /// </summary>
        public static void ResumeAll()
        {
            AudioManager.Instance.ResumeAll();
        }

        #endregion

        #region Query

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public static bool IsPlaying(string clipName)
        {
            return AudioManager.Instance.IsPlaying(clipName);
        }

        /// <summary>
        /// 获取正在播放的实例数
        /// </summary>
        public static int GetPlayingCount(string clipName)
        {
            return AudioManager.Instance.GetPlayingCount(clipName);
        }

        /// <summary>
        /// 获取指定实例
        /// </summary>
        public static ActiveAudioInstance GetInstance(int instanceId)
        {
            return AudioManager.Instance.GetInstance(instanceId);
        }

        /// <summary>
        /// 检查音效是否存在于已注册的 Bank 中
        /// </summary>
        /// <param name="nameOrEvent">音效名称或事件名称</param>
        /// <returns>如果音效存在返回 true，否则返回 false</returns>
        public static bool HasAudio(string nameOrEvent)
        {
            return AudioManager.Instance.HasAudio(nameOrEvent);
        }

        #endregion

        #region Bank/Group Management

        /// <summary>
        /// 注册 AudioBank
        /// </summary>
        public static void RegisterBank(AudioBank bank)
        {
            AudioManager.Instance.RegisterBank(bank);
        }

        /// <summary>
        /// 注销 AudioBank
        /// </summary>
        public static void UnregisterBank(AudioBank bank)
        {
            AudioManager.Instance.UnregisterBank(bank);
        }

        /// <summary>
        /// 注册 AudioGroup
        /// </summary>
        public static void RegisterGroup(AudioGroup group)
        {
            AudioManager.Instance.RegisterGroup(group);
        }

        /// <summary>
        /// 注销 AudioGroup
        /// </summary>
        public static void UnregisterGroup(AudioGroup group)
        {
            AudioManager.Instance.UnregisterGroup(group);
        }

        /// <summary>
        /// 预加载 Bank 中的所有音效（同步）
        /// </summary>
        public static void PreloadBank(AudioBank bank)
        {
            AudioManager.Instance.PreloadBank(bank);
        }

        /// <summary>
        /// 预加载 Bank 中的所有音效（异步）
        /// </summary>
        /// <param name="bank">AudioBank</param>
        /// <param name="onComplete">全部加载完成回调</param>
        public static void PreloadBankAsync(AudioBank bank, Action onComplete = null)
        {
            AudioManager.Instance.PreloadBankAsync(bank, onComplete);
        }

        #endregion

        #region Instance Control

        /// <summary>
        /// 设置实例音量
        /// </summary>
        public static void SetInstanceVolume(int instanceId, float volume)
        {
            var instance = AudioManager.Instance.GetInstance(instanceId);
            if (instance != null)
            {
                instance.SetVolume(volume);
            }
        }

        /// <summary>
        /// 设置实例音调
        /// </summary>
        public static void SetInstancePitch(int instanceId, float pitch)
        {
            var instance = AudioManager.Instance.GetInstance(instanceId);
            if (instance != null)
            {
                instance.SetPitch(pitch);
            }
        }

        #endregion
    }
}
