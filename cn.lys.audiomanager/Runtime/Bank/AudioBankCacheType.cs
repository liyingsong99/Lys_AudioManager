using Sirenix.OdinInspector;

namespace Lys.Audio
{
    /// <summary>
    /// AudioBank 缓存类型
    /// </summary>
    public enum AudioBankCacheType
    {
        [LabelText("按需加载")]
        OnDemand = 0,

        [LabelText("预加载")]
        Preload = 1,

        [LabelText("常驻内存")]
        Persistent = 2
    }
}
