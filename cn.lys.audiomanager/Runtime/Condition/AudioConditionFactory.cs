using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// 音效播放条件工厂 - 用于创建和注册自定义条件
    /// </summary>
    public static class AudioConditionFactory
    {
        private static readonly Dictionary<string, Type> conditionTypes = new Dictionary<string, Type>();
        private static readonly Dictionary<string, string> conditionDisplayNames = new Dictionary<string, string>();

        static AudioConditionFactory()
        {
            RegisterBuiltinConditions();
        }

        private static void RegisterBuiltinConditions()
        {
            RegisterCondition<ConcurrentLimitCondition>("ConcurrentLimit", "同时播放上限");
            RegisterCondition<CooldownCondition>("Cooldown", "冷却时间");
            RegisterCondition<ProbabilityCondition>("Probability", "概率播放");
            RegisterCondition<DistanceCondition>("Distance", "距离条件");
        }

        public static void RegisterCondition<T>(string conditionId, string displayName) where T : IAudioPlayCondition, new()
        {
            if (string.IsNullOrEmpty(conditionId))
            {
                Debug.LogError("[AudioConditionFactory] conditionId cannot be null or empty");
                return;
            }

            conditionTypes[conditionId] = typeof(T);
            conditionDisplayNames[conditionId] = displayName;
        }

        public static void RegisterCondition(string conditionId, Type conditionType, string displayName)
        {
            if (string.IsNullOrEmpty(conditionId))
            {
                Debug.LogError("[AudioConditionFactory] conditionId cannot be null or empty");
                return;
            }

            if (conditionType == null || !typeof(IAudioPlayCondition).IsAssignableFrom(conditionType))
            {
                Debug.LogError($"[AudioConditionFactory] Invalid condition type: {conditionType}");
                return;
            }

            conditionTypes[conditionId] = conditionType;
            conditionDisplayNames[conditionId] = displayName;
        }

        public static void UnregisterCondition(string conditionId)
        {
            conditionTypes.Remove(conditionId);
            conditionDisplayNames.Remove(conditionId);
        }

        public static IAudioPlayCondition CreateCondition(string conditionId)
        {
            if (!conditionTypes.TryGetValue(conditionId, out var type))
            {
                Debug.LogError($"[AudioConditionFactory] Unknown condition type: {conditionId}");
                return null;
            }

            try
            {
                return Activator.CreateInstance(type) as IAudioPlayCondition;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AudioConditionFactory] Failed to create condition: {conditionId}, Error: {e.Message}");
                return null;
            }
        }

        public static T CreateCondition<T>() where T : IAudioPlayCondition, new()
        {
            return new T();
        }

        public static IEnumerable<string> GetRegisteredConditionIds()
        {
            return conditionTypes.Keys;
        }

        public static string GetDisplayName(string conditionId)
        {
            if (conditionDisplayNames.TryGetValue(conditionId, out var name))
            {
                return name;
            }
            return conditionId;
        }

        public static Type GetConditionType(string conditionId)
        {
            conditionTypes.TryGetValue(conditionId, out var type);
            return type;
        }

        public static bool IsRegistered(string conditionId)
        {
            return conditionTypes.ContainsKey(conditionId);
        }

        public static Dictionary<string, string> GetAllConditionInfo()
        {
            return new Dictionary<string, string>(conditionDisplayNames);
        }
    }
}
