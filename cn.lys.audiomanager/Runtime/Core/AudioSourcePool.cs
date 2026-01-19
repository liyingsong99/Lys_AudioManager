using System.Collections.Generic;
using UnityEngine;

namespace Lys.Audio
{
    /// <summary>
    /// AudioSource 对象池
    /// </summary>
    public class AudioSourcePool
    {
        private readonly Queue<AudioSource> availableSources = new Queue<AudioSource>();
        private readonly HashSet<AudioSource> activeSources = new HashSet<AudioSource>();
        private readonly Transform poolRoot;
        private readonly int maxPoolSize;
        private readonly string poolName;

        public int AvailableCount => availableSources.Count;
        public int ActiveCount => activeSources.Count;
        public int TotalCount => AvailableCount + ActiveCount;

        public AudioSourcePool(Transform parent, int maxSize = 32, string name = "AudioSourcePool")
        {
            this.maxPoolSize = maxSize;
            this.poolName = name;

            GameObject poolObj = new GameObject(name);
            poolRoot = poolObj.transform;
            poolRoot.SetParent(parent);
            poolRoot.localPosition = Vector3.zero;
        }

        public AudioSource Get(Vector3? position = null)
        {
            AudioSource source;

            if (availableSources.Count > 0)
            {
                source = availableSources.Dequeue();
                if (source == null)
                {
                    source = CreateNewSource();
                }
            }
            else if (TotalCount < maxPoolSize)
            {
                source = CreateNewSource();
            }
            else
            {
                source = ForceRecycleOldest();
                if (source == null)
                {
                    source = CreateNewSource();
                }
            }

            ResetSource(source);
            source.gameObject.SetActive(true);

            if (position.HasValue)
            {
                source.transform.position = position.Value;
            }

            activeSources.Add(source);
            return source;
        }

        public void Return(AudioSource source)
        {
            if (source == null) return;

            if (!activeSources.Contains(source))
            {
                GameObject.Destroy(source.gameObject);
                return;
            }

            activeSources.Remove(source);

            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            source.transform.SetParent(poolRoot);
            source.transform.localPosition = Vector3.zero;

            availableSources.Enqueue(source);
        }

        public void Clear()
        {
            foreach (var source in activeSources)
            {
                if (source != null)
                {
                    source.Stop();
                    GameObject.Destroy(source.gameObject);
                }
            }
            activeSources.Clear();

            while (availableSources.Count > 0)
            {
                var source = availableSources.Dequeue();
                if (source != null)
                {
                    GameObject.Destroy(source.gameObject);
                }
            }
        }

        public void Destroy()
        {
            Clear();
            if (poolRoot != null)
            {
                GameObject.Destroy(poolRoot.gameObject);
            }
        }

        public void Warmup(int count)
        {
            count = Mathf.Min(count, maxPoolSize - TotalCount);
            for (int i = 0; i < count; i++)
            {
                var source = CreateNewSource();
                source.gameObject.SetActive(false);
                availableSources.Enqueue(source);
            }
        }

        private AudioSource CreateNewSource()
        {
            GameObject obj = new GameObject($"AudioSource_{TotalCount}");
            obj.transform.SetParent(poolRoot);
            obj.transform.localPosition = Vector3.zero;

            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;

            return source;
        }

        private void ResetSource(AudioSource source)
        {
            source.clip = null;
            source.volume = 1f;
            source.pitch = 1f;
            source.loop = false;
            source.spatialBlend = 0f;
            source.minDistance = 1f;
            source.maxDistance = 500f;
            source.dopplerLevel = 1f;
            source.mute = false;
            source.outputAudioMixerGroup = null;
            source.priority = 128;
            source.reverbZoneMix = 1f;
            source.bypassEffects = false;
            source.bypassListenerEffects = false;
            source.bypassReverbZones = false;
        }

        private AudioSource ForceRecycleOldest()
        {
            AudioSource oldest = null;
            float oldestTime = float.MaxValue;

            foreach (var source in activeSources)
            {
                if (source == null) continue;

                if (!source.loop && source.time < oldestTime)
                {
                    oldest = source;
                    oldestTime = source.time;
                }
            }

            if (oldest != null)
            {
                activeSources.Remove(oldest);
                oldest.Stop();
                oldest.clip = null;
                return oldest;
            }

            return null;
        }
    }
}
