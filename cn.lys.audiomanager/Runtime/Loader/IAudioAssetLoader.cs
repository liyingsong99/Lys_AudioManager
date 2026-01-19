using System;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// 音效资源加载器接口
    /// </summary>
    public interface IAudioAssetLoader
    {
        void LoadClipAsync(string assetPath, Action<AudioClip> onComplete);
        AudioClip LoadClipSync(string assetPath);
        void UnloadClip(string assetPath);
        void UnloadAll();
        bool IsLoaded(string assetPath);
        AudioClip GetLoadedClip(string assetPath);
    }
}
