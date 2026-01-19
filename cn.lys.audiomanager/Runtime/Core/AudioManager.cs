using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// 音效管理器
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Singleton

        private static AudioManager instance;

        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    CreateInstance();
                }
                return instance;
            }
        }

        public static bool IsInitialized => instance != null;

        private static void CreateInstance()
        {
            if (instance != null) return;

            GameObject obj = new GameObject("[AudioManager]");
            instance = obj.AddComponent<AudioManager>();
            DontDestroyOnLoad(obj);
        }

        #endregion

        #region Fields

        private AudioSourcePool sourcePool;
        private IAudioAssetLoader assetLoader;

        private readonly Dictionary<string, HashSet<ActiveAudioInstance>> activeInstances = new Dictionary<string, HashSet<ActiveAudioInstance>>();
        private readonly Dictionary<int, ActiveAudioInstance> instancesById = new Dictionary<int, ActiveAudioInstance>();

        private readonly Dictionary<string, AudioBank> registeredBanks = new Dictionary<string, AudioBank>();
        private readonly Dictionary<string, AudioGroup> registeredGroups = new Dictionary<string, AudioGroup>();

        private readonly Dictionary<string, AudioClipEntry> clipEntryCache = new Dictionary<string, AudioClipEntry>();
        private readonly Dictionary<string, AudioBank> clipToBankCache = new Dictionary<string, AudioBank>();

        private readonly Dictionary<string, AudioClipEntry> eventNameCache = new Dictionary<string, AudioClipEntry>();
        private readonly Dictionary<string, AudioBank> eventToBankCache = new Dictionary<string, AudioBank>();

        private readonly Dictionary<string, float> lastPlayTimeCache = new Dictionary<string, float>();

        private readonly List<ActiveAudioInstance> pendingRemove = new List<ActiveAudioInstance>();

        private readonly Dictionary<string, List<string>> playGroupMembers = new Dictionary<string, List<string>>();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            Initialize();
        }

        private void Update()
        {
            UpdateActiveInstances();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                Cleanup();
                instance = null;
            }
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            var settings = AudioManagerSettings.Instance;

            sourcePool = new AudioSourcePool(transform, settings.maxAudioSources, "AudioSourcePool");
            sourcePool.Warmup(settings.warmupCount);

            assetLoader = new YooAssetAudioLoader();

            if (settings.enableDebugLog)
            {
                Debug.Log("[AudioManager] Initialized");
            }
        }

        private void Cleanup()
        {
            StopAllImmediate();
            sourcePool?.Destroy();
            assetLoader?.UnloadAll();

            registeredBanks.Clear();
            registeredGroups.Clear();
            clipEntryCache.Clear();
            clipToBankCache.Clear();
            eventNameCache.Clear();
            eventToBankCache.Clear();
            lastPlayTimeCache.Clear();
            playGroupMembers.Clear();

            if (AudioManagerSettings.Instance.enableDebugLog)
            {
                Debug.Log("[AudioManager] Cleaned up");
            }
        }

        #endregion

        #region Public API - Play

        public ActiveAudioInstance Play(string clipName, Vector3? position = null, AudioClipParameters overrideParams = null)
        {
            if (string.IsNullOrEmpty(clipName))
            {
                Debug.LogWarning("[AudioManager] Play: clipName is null or empty");
                return null;
            }

            if (!TryGetClipEntry(clipName, out var entry, out var bank))
            {
                Debug.LogWarning($"[AudioManager] Audio clip not found: {clipName}. Registered clips: {string.Join(", ", clipEntryCache.Keys)}");
                return null;
            }

            string actualClipName = ResolvePlayGroupClip(clipName, entry);
            if (actualClipName == null)
            {
                return null;
            }
            if (actualClipName != clipName)
            {
                if (!TryGetClipEntry(actualClipName, out entry, out bank))
                {
                    Debug.LogWarning($"[AudioManager] PlayGroup resolved clip not found: {actualClipName}");
                    return null;
                }
                clipName = actualClipName;
            }

            var context = CreateConditionContext(clipName, entry, bank);
            if (!EvaluateConditions(entry, context))
            {
                if (AudioManagerSettings.Instance.enableDebugLog)
                {
                    Debug.Log($"[AudioManager] Play blocked by conditions: {clipName}");
                }
                return null;
            }

            AudioClip clip = entry.loadedClip;
            if (clip == null)
            {
                clip = assetLoader.LoadClipSync(entry.assetPath);
                if (clip == null)
                {
                    Debug.LogError($"[AudioManager] Failed to load audio clip: {entry.assetPath}");
                    return null;
                }
                entry.loadedClip = clip;
            }

            return PlayInternal(clipName, clip, entry, bank, position, overrideParams);
        }

        public void PlayAsync(string clipName, Action<ActiveAudioInstance> onComplete, Vector3? position = null, AudioClipParameters overrideParams = null)
        {
            if (string.IsNullOrEmpty(clipName))
            {
                Debug.LogWarning("[AudioManager] PlayAsync: clipName is null or empty");
                onComplete?.Invoke(null);
                return;
            }

            if (!TryGetClipEntry(clipName, out var entry, out var bank))
            {
                Debug.LogWarning($"[AudioManager] Audio clip not found: {clipName}");
                onComplete?.Invoke(null);
                return;
            }

            string actualClipName = ResolvePlayGroupClip(clipName, entry);
            if (actualClipName == null)
            {
                onComplete?.Invoke(null);
                return;
            }
            if (actualClipName != clipName)
            {
                if (!TryGetClipEntry(actualClipName, out entry, out bank))
                {
                    Debug.LogWarning($"[AudioManager] PlayGroup resolved clip not found: {actualClipName}");
                    onComplete?.Invoke(null);
                    return;
                }
                clipName = actualClipName;
            }

            var context = CreateConditionContext(clipName, entry, bank);
            if (!EvaluateConditions(entry, context))
            {
                if (AudioManagerSettings.Instance.enableDebugLog)
                {
                    Debug.Log($"[AudioManager] Play blocked by conditions: {clipName}");
                }
                onComplete?.Invoke(null);
                return;
            }

            if (entry.loadedClip != null)
            {
                var inst = PlayInternal(clipName, entry.loadedClip, entry, bank, position, overrideParams);
                onComplete?.Invoke(inst);
                return;
            }

            assetLoader.LoadClipAsync(entry.assetPath, (AudioClip clip) =>
            {
                if (clip == null)
                {
                    Debug.LogError($"[AudioManager] Failed to load audio clip: {entry.assetPath}");
                    onComplete?.Invoke(null);
                    return;
                }

                entry.loadedClip = clip;
                var inst = PlayInternal(clipName, clip, entry, bank, position, overrideParams);
                onComplete?.Invoke(inst);
            });
        }

        public ActiveAudioInstance PlayFollow(string clipName, Transform target, AudioClipParameters overrideParams = null)
        {
            var inst = Play(clipName, target?.position, overrideParams);
            if (inst != null)
            {
                inst.FollowTarget = target;
            }
            return inst;
        }

        public void PlayFollowAsync(string clipName, Transform target, Action<ActiveAudioInstance> onComplete, AudioClipParameters overrideParams = null)
        {
            PlayAsync(clipName, (ActiveAudioInstance inst) =>
            {
                if (inst != null)
                {
                    inst.FollowTarget = target;
                }
                onComplete?.Invoke(inst);
            }, target?.position, overrideParams);
        }

        private ActiveAudioInstance PlayInternal(string clipName, AudioClip clip, AudioClipEntry entry, AudioBank bank, Vector3? position, AudioClipParameters overrideParams)
        {
            var parameters = overrideParams ?? entry.GetParameters(bank?.DefaultParameters);

            var source = sourcePool.Get(position);
            if (source == null)
            {
                Debug.LogError("[AudioManager] Failed to get AudioSource from pool");
                return null;
            }

            var group = GetGroupForBank(bank);
            if (group?.MixerGroup != null)
            {
                source.outputAudioMixerGroup = group.MixerGroup;
            }

            var inst = new ActiveAudioInstance(clipName, source, clip, parameters);

            AddActiveInstance(inst);

            var context = CreateConditionContext(clipName, entry, bank);
            NotifyConditionsPlayStart(entry, context);

            inst.Play();

            lastPlayTimeCache[clipName] = Time.time;

            if (AudioManagerSettings.Instance.enablePlayLog)
            {
                Debug.Log($"[AudioManager] Playing: {clipName}");
            }

            return inst;
        }

        #endregion

        #region Public API - Stop

        public void Stop(string clipName, bool fadeOut = true)
        {
            if (!activeInstances.TryGetValue(clipName, out var instances)) return;

            foreach (var inst in instances)
            {
                inst.Stop(fadeOut);
            }
        }

        public void Stop(string clipName, float fadeOutDuration)
        {
            if (!activeInstances.TryGetValue(clipName, out var instances)) return;

            foreach (var inst in instances)
            {
                inst.Stop(fadeOutDuration);
            }
        }

        public void StopAll(bool fadeOut = true, bool onlyLooping = false)
        {
            foreach (var kvp in activeInstances)
            {
                foreach (var inst in kvp.Value)
                {
                    if (onlyLooping && !inst.Parameters.loop) continue;
                    inst.Stop(fadeOut);
                }
            }
        }

        public void StopAllImmediate()
        {
            foreach (var kvp in activeInstances)
            {
                foreach (var inst in kvp.Value)
                {
                    inst.StopImmediate();
                }
            }

            activeInstances.Clear();
            instancesById.Clear();
        }

        public void StopInstance(int instanceId, bool fadeOut = true)
        {
            if (instancesById.TryGetValue(instanceId, out var inst))
            {
                inst.Stop(fadeOut);
            }
        }

        #endregion

        #region Public API - Control

        public void Pause(string clipName)
        {
            if (!activeInstances.TryGetValue(clipName, out var instances)) return;

            foreach (var inst in instances)
            {
                inst.Pause();
            }
        }

        public void Resume(string clipName)
        {
            if (!activeInstances.TryGetValue(clipName, out var instances)) return;

            foreach (var inst in instances)
            {
                inst.Resume();
            }
        }

        public void PauseAll()
        {
            foreach (var kvp in activeInstances)
            {
                foreach (var inst in kvp.Value)
                {
                    inst.Pause();
                }
            }
        }

        public void ResumeAll()
        {
            foreach (var kvp in activeInstances)
            {
                foreach (var inst in kvp.Value)
                {
                    inst.Resume();
                }
            }
        }

        #endregion

        #region Public API - Query

        public bool IsPlaying(string clipName)
        {
            if (!activeInstances.TryGetValue(clipName, out var instances)) return false;

            foreach (var inst in instances)
            {
                if (inst.IsPlaying) return true;
            }
            return false;
        }

        public int GetPlayingCount(string clipName)
        {
            if (!activeInstances.TryGetValue(clipName, out var instances)) return 0;

            int count = 0;
            foreach (var inst in instances)
            {
                if (inst.IsPlaying) count++;
            }
            return count;
        }

        public ActiveAudioInstance GetInstance(int instanceId)
        {
            instancesById.TryGetValue(instanceId, out var inst);
            return inst;
        }

        public IEnumerable<ActiveAudioInstance> GetActiveInstances()
        {
            return instancesById.Values;
        }

        public bool HasAudio(string nameOrEvent)
        {
            if (string.IsNullOrEmpty(nameOrEvent))
            {
                return false;
            }
            return TryGetClipEntry(nameOrEvent, out _, out _);
        }

        #endregion

        #region Public API - Bank/Group Management

        public void RegisterBank(AudioBank bank)
        {
            if (bank == null) return;

            registeredBanks[bank.BankName] = bank;

            foreach (var entry in bank.GetAllClips())
            {
                clipEntryCache[entry.clipName] = entry;
                clipToBankCache[entry.clipName] = bank;

                if (!string.IsNullOrEmpty(entry.eventName))
                {
                    if (eventNameCache.ContainsKey(entry.eventName))
                    {
                        Debug.LogWarning($"[AudioManager] Duplicate eventName: {entry.eventName}, will be overwritten by {entry.clipName}");
                    }
                    eventNameCache[entry.eventName] = entry;
                    eventToBankCache[entry.eventName] = bank;
                }
            }

            if (bank.CacheType == AudioBankCacheType.Preload)
            {
                PreloadBank(bank);
            }

            BuildPlayGroupIndex(bank);

            if (AudioManagerSettings.Instance.enableDebugLog)
            {
                Debug.Log($"[AudioManager] Registered bank: {bank.BankName}");
            }
        }

        public void UnregisterBank(AudioBank bank)
        {
            if (bank == null) return;

            registeredBanks.Remove(bank.BankName);

            foreach (var entry in bank.GetAllClips())
            {
                clipEntryCache.Remove(entry.clipName);
                clipToBankCache.Remove(entry.clipName);

                if (!string.IsNullOrEmpty(entry.eventName))
                {
                    eventNameCache.Remove(entry.eventName);
                    eventToBankCache.Remove(entry.eventName);
                }

                if (entry.IsLoaded)
                {
                    assetLoader.UnloadClip(entry.assetPath);
                    entry.loadedClip = null;
                }
            }

            CleanupPlayGroupIndex(bank);

            if (AudioManagerSettings.Instance.enableDebugLog)
            {
                Debug.Log($"[AudioManager] Unregistered bank: {bank.BankName}");
            }
        }

        public void RegisterGroup(AudioGroup group)
        {
            if (group == null) return;

            registeredGroups[group.GroupName] = group;

            foreach (var bank in group.AudioBanks)
            {
                RegisterBank(bank);
            }

            if (AudioManagerSettings.Instance.enableDebugLog)
            {
                Debug.Log($"[AudioManager] Registered group: {group.GroupName}");
            }
        }

        public void UnregisterGroup(AudioGroup group)
        {
            if (group == null) return;

            registeredGroups.Remove(group.GroupName);

            if (AudioManagerSettings.Instance.enableDebugLog)
            {
                Debug.Log($"[AudioManager] Unregistered group: {group.GroupName}");
            }
        }

        public void PreloadBank(AudioBank bank)
        {
            if (bank == null) return;

            foreach (var entry in bank.GetAllClips())
            {
                if (!entry.IsLoaded)
                {
                    entry.loadedClip = assetLoader.LoadClipSync(entry.assetPath);
                }
            }
        }

        public void PreloadBankAsync(AudioBank bank, Action onComplete = null)
        {
            if (bank == null)
            {
                onComplete?.Invoke();
                return;
            }

            var clips = new List<AudioClipEntry>();
            foreach (var entry in bank.GetAllClips())
            {
                if (!entry.IsLoaded)
                {
                    clips.Add(entry);
                }
            }

            if (clips.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            int loadedCount = 0;
            foreach (var entry in clips)
            {
                assetLoader.LoadClipAsync(entry.assetPath, (AudioClip clip) =>
                {
                    entry.loadedClip = clip;
                    loadedCount++;

                    if (loadedCount >= clips.Count)
                    {
                        onComplete?.Invoke();
                    }
                });
            }
        }

        #endregion

        #region Private Methods

        private void UpdateActiveInstances()
        {
            pendingRemove.Clear();

            foreach (var kvp in activeInstances)
            {
                foreach (var inst in kvp.Value)
                {
                    inst.Update();

                    if (!inst.IsPlaying && !inst.IsPaused)
                    {
                        pendingRemove.Add(inst);
                    }
                }
            }

            foreach (var inst in pendingRemove)
            {
                RemoveActiveInstance(inst);

                sourcePool.Return(inst.Source);

                if (TryGetClipEntry(inst.ClipName, out var entry, out var bank))
                {
                    var context = CreateConditionContext(inst.ClipName, entry, bank);
                    NotifyConditionsPlayStop(entry, context);
                }
            }
        }

        private void AddActiveInstance(ActiveAudioInstance inst)
        {
            if (!activeInstances.TryGetValue(inst.ClipName, out var set))
            {
                set = new HashSet<ActiveAudioInstance>();
                activeInstances[inst.ClipName] = set;
            }
            set.Add(inst);
            instancesById[inst.Id] = inst;
        }

        private void RemoveActiveInstance(ActiveAudioInstance inst)
        {
            if (activeInstances.TryGetValue(inst.ClipName, out var set))
            {
                set.Remove(inst);
                if (set.Count == 0)
                {
                    activeInstances.Remove(inst.ClipName);
                }
            }
            instancesById.Remove(inst.Id);
        }

        private bool TryGetClipEntry(string nameOrEvent, out AudioClipEntry entry, out AudioBank bank)
        {
            if (eventNameCache.TryGetValue(nameOrEvent, out entry))
            {
                eventToBankCache.TryGetValue(nameOrEvent, out bank);
                return true;
            }

            if (clipEntryCache.TryGetValue(nameOrEvent, out entry))
            {
                clipToBankCache.TryGetValue(nameOrEvent, out bank);
                return true;
            }

            entry = null;
            bank = null;
            return false;
        }

        private AudioGroup GetGroupForBank(AudioBank bank)
        {
            if (bank == null) return null;

            foreach (var group in registeredGroups.Values)
            {
                foreach (var b in group.AudioBanks)
                {
                    if (b == bank)
                    {
                        return group;
                    }
                }
            }
            return null;
        }

        private AudioConditionContext CreateConditionContext(string clipName, AudioClipEntry entry, AudioBank bank)
        {
            lastPlayTimeCache.TryGetValue(clipName, out var lastPlayTime);

            return new AudioConditionContext
            {
                ClipName = clipName,
                ClipEntry = entry,
                Bank = bank,
                Group = GetGroupForBank(bank),
                Manager = this,
                CurrentPlayingCount = GetPlayingCount(clipName),
                LastPlayTime = lastPlayTime
            };
        }

        private bool EvaluateConditions(AudioClipEntry entry, AudioConditionContext context)
        {
            if (entry.playConditions == null || entry.playConditions.Length == 0)
            {
                return true;
            }

            bool result = entry.conditionOperator == AudioConditionOperator.And;

            foreach (var condition in entry.playConditions)
            {
                if (condition == null) continue;

                bool conditionResult = condition.Evaluate(context);

                if (entry.conditionOperator == AudioConditionOperator.And)
                {
                    result = result && conditionResult;
                    if (!result) break;
                }
                else
                {
                    result = result || conditionResult;
                    if (result) break;
                }
            }

            return result;
        }

        private void NotifyConditionsPlayStart(AudioClipEntry entry, AudioConditionContext context)
        {
            if (entry.playConditions == null) return;

            foreach (var condition in entry.playConditions)
            {
                condition?.OnPlayStart(context);
            }
        }

        private void NotifyConditionsPlayStop(AudioClipEntry entry, AudioConditionContext context)
        {
            if (entry.playConditions == null) return;

            foreach (var condition in entry.playConditions)
            {
                condition?.OnPlayStop(context);
            }
        }

        #endregion

        #region PlayGroup Methods

        private void BuildPlayGroupIndex(AudioBank bank)
        {
            foreach (var entry in bank.GetAllClips())
            {
                if (string.IsNullOrEmpty(entry?.playGroupName))
                {
                    continue;
                }

                var groupName = entry.playGroupName;

                if (!playGroupMembers.TryGetValue(groupName, out var members))
                {
                    members = new List<string>();
                    playGroupMembers[groupName] = members;
                }

                if (!members.Contains(entry.clipName))
                {
                    members.Add(entry.clipName);
                }
            }
        }

        private void CleanupPlayGroupIndex(AudioBank bank)
        {
            foreach (var entry in bank.GetAllClips())
            {
                if (string.IsNullOrEmpty(entry?.playGroupName))
                {
                    continue;
                }

                var groupName = entry.playGroupName;
                if (playGroupMembers.TryGetValue(groupName, out var members))
                {
                    members.Remove(entry.clipName);
                    if (members.Count == 0)
                    {
                        playGroupMembers.Remove(groupName);
                    }
                }
            }
        }

        private string ResolvePlayGroupClip(string clipName, AudioClipEntry entry)
        {
            if (string.IsNullOrEmpty(entry?.playGroupName))
            {
                return clipName;
            }

            var groupName = entry.playGroupName;

            var playGroupSettings = AudioPlayGroupSettings.Instance;
            if (playGroupSettings == null)
            {
                return clipName;
            }

            var playGroup = playGroupSettings.GetGroup(groupName);
            if (playGroup == null)
            {
                return clipName;
            }

            if (!playGroupMembers.TryGetValue(groupName, out var members) || members.Count == 0)
            {
                return clipName;
            }

            if (playGroup.mode == PlayGroupMode.Exclusive)
            {
                ActiveAudioInstance playingInstance = null;

                foreach (var inst in GetActiveInstances())
                {
                    if (!inst.IsPlaying)
                    {
                        continue;
                    }

                    if (TryGetClipEntry(inst.ClipName, out var playingEntry, out _))
                    {
                        if (playingEntry?.playGroupName == groupName)
                        {
                            playingInstance = inst;
                            break;
                        }
                    }
                }

                if (playingInstance != null)
                {
                    switch (playGroup.exclusiveBehavior)
                    {
                        case ExclusiveBehavior.DontPlay:
                            if (AudioManagerSettings.Instance.enableDebugLog)
                            {
                                Debug.Log($"[AudioManager] PlayGroup Exclusive: {clipName} blocked by {playingInstance.ClipName}");
                            }
                            return null;

                        case ExclusiveBehavior.StopOld:
                            if (AudioManagerSettings.Instance.enableDebugLog)
                            {
                                Debug.Log($"[AudioManager] PlayGroup Exclusive: stopping {playingInstance.ClipName} to play {clipName}");
                            }
                            playingInstance.StopImmediate();
                            break;

                        case ExclusiveBehavior.FadeOutOld:
                            if (AudioManagerSettings.Instance.enableDebugLog)
                            {
                                Debug.Log($"[AudioManager] PlayGroup Exclusive: fading out {playingInstance.ClipName} to play {clipName}");
                            }
                            playingInstance.Stop(true);
                            break;
                    }
                }
            }

            var selectedClip = playGroup.SelectNextClip(members, clipName);

            if (AudioManagerSettings.Instance.enableDebugLog)
            {
                Debug.Log($"[AudioManager] PlayGroup '{groupName}' selected: {selectedClip} (mode: {playGroup.mode})");
            }

            return selectedClip ?? clipName;
        }

        #endregion
    }
}
