using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace Lys.Audio
{
    /// <summary>
    /// YooAsset 音效加载器实现
    /// </summary>
    public class YooAssetAudioLoader : IAudioAssetLoader
    {
        private readonly Dictionary<string, AssetHandle> loadedHandles = new Dictionary<string, AssetHandle>();
        private readonly Dictionary<string, AudioClip> loadedClips = new Dictionary<string, AudioClip>();

        public void LoadClipAsync(string assetPath, Action<AudioClip> onComplete)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("[AudioManager] LoadClipAsync: assetPath is null or empty");
                onComplete?.Invoke(null);
                return;
            }

            if (loadedClips.TryGetValue(assetPath, out var existingClip))
            {
                onComplete?.Invoke(existingClip);
                return;
            }

            try
            {
                var handle = YooAssets.LoadAssetAsync<AudioClip>(assetPath);
                handle.Completed += (AssetHandle h) =>
                {
                    if (h.Status != EOperationStatus.Succeed)
                    {
                        Debug.LogError($"[AudioManager] Failed to load audio clip: {assetPath}, Error: {h.LastError}");
                        onComplete?.Invoke(null);
                        return;
                    }

                    var clip = h.AssetObject as AudioClip;
                    if (clip == null)
                    {
                        Debug.LogError($"[AudioManager] Loaded asset is not AudioClip: {assetPath}");
                        h.Release();
                        onComplete?.Invoke(null);
                        return;
                    }

                    loadedHandles[assetPath] = h;
                    loadedClips[assetPath] = clip;

                    if (AudioManagerSettings.Instance.enableDebugLog)
                    {
                        Debug.Log($"[AudioManager] Loaded audio clip: {assetPath}");
                    }

                    onComplete?.Invoke(clip);
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"[AudioManager] Exception loading audio clip: {assetPath}, {e.Message}");
                onComplete?.Invoke(null);
            }
        }

        public AudioClip LoadClipSync(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("[AudioManager] LoadClipSync: assetPath is null or empty");
                return null;
            }

            if (loadedClips.TryGetValue(assetPath, out var existingClip))
            {
                return existingClip;
            }

            try
            {
                var handle = YooAssets.LoadAssetSync<AudioClip>(assetPath);

                if (handle.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"[AudioManager] Failed to load audio clip: {assetPath}, Error: {handle.LastError}");
                    return null;
                }

                var clip = handle.AssetObject as AudioClip;
                if (clip == null)
                {
                    Debug.LogError($"[AudioManager] Loaded asset is not AudioClip: {assetPath}");
                    handle.Release();
                    return null;
                }

                loadedHandles[assetPath] = handle;
                loadedClips[assetPath] = clip;

                if (AudioManagerSettings.Instance.enableDebugLog)
                {
                    Debug.Log($"[AudioManager] Loaded audio clip (sync): {assetPath}");
                }

                return clip;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AudioManager] Exception loading audio clip: {assetPath}, {e.Message}");
                return null;
            }
        }

        public void UnloadClip(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;

            if (loadedHandles.TryGetValue(assetPath, out var handle))
            {
                handle.Release();
                loadedHandles.Remove(assetPath);
                loadedClips.Remove(assetPath);

                if (AudioManagerSettings.Instance.enableDebugLog)
                {
                    Debug.Log($"[AudioManager] Unloaded audio clip: {assetPath}");
                }
            }
        }

        public void UnloadAll()
        {
            foreach (var handle in loadedHandles.Values)
            {
                handle.Release();
            }
            loadedHandles.Clear();
            loadedClips.Clear();

            if (AudioManagerSettings.Instance.enableDebugLog)
            {
                Debug.Log("[AudioManager] Unloaded all audio clips");
            }
        }

        public bool IsLoaded(string assetPath)
        {
            return loadedClips.ContainsKey(assetPath);
        }

        public AudioClip GetLoadedClip(string assetPath)
        {
            loadedClips.TryGetValue(assetPath, out var clip);
            return clip;
        }
    }
}
