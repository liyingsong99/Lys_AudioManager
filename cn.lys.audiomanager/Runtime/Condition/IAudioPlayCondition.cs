namespace Lys.Audio
{
    /// <summary>
    /// 音效播放条件接口
    /// </summary>
    public interface IAudioPlayCondition
    {
        string ConditionName { get; }
        string Description { get; }
        bool Evaluate(AudioConditionContext context);
        void OnPlayStart(AudioConditionContext context);
        void OnPlayStop(AudioConditionContext context);
    }
}
