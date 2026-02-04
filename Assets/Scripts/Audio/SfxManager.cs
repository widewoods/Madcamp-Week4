using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SfxManager : MonoBehaviour
{
  public static SfxManager Instance { get; private set; }

  [Header("References")]
  [SerializeField] private SfxLibrary library;
  [SerializeField] private AudioMixerGroup defaultMixerGroup;

  [Header("Pool")]
  [SerializeField] private int initialPoolSize = 8;
  [SerializeField] private bool persistAcrossScenes = true;

  private readonly List<AudioSource> pool = new List<AudioSource>();

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    if (persistAcrossScenes)
      DontDestroyOnLoad(gameObject);

    WarmPool();
  }

  private void WarmPool()
  {
    for (int i = pool.Count; i < initialPoolSize; i++)
      pool.Add(CreateSource());
  }

  private AudioSource CreateSource()
  {
    var go = new GameObject("SfxSource");
    go.transform.SetParent(transform, false);
    var src = go.AddComponent<AudioSource>();
    src.playOnAwake = false;
    src.loop = false;
    return src;
  }

  private AudioSource GetAvailableSource()
  {
    foreach (var src in pool)
      if (src != null && !src.isPlaying)
        return src;

    var created = CreateSource();
    pool.Add(created);
    return created;
  }

  public void Play2D(SfxId id, float volumeScale = 1f)
  {
    PlayInternal(id, Vector3.zero, false, volumeScale);
  }

  public void Play3D(SfxId id, Vector3 position, float volumeScale = 1f)
  {
    PlayInternal(id, position, true, volumeScale);
  }

  private void PlayInternal(SfxId id, Vector3 position, bool usePosition, float volumeScale)
  {
    if (library == null)
    {
      Debug.LogWarning("[SfxManager] Missing SfxLibrary reference.");
      return;
    }

    if (!library.TryGetEntry(id, out var entry))
    {
      Debug.LogWarning($"[SfxManager] Missing SFX entry for {id}.");
      return;
    }

    var clip = library.PickClip(entry);
    if (clip == null) return;

    var src = GetAvailableSource();
    if (usePosition)
      src.transform.position = position;

    src.clip = clip;
    src.volume = Mathf.Clamp01(entry.volume * volumeScale);
    src.pitch = Mathf.Clamp(Random.Range(entry.pitchRange.x, entry.pitchRange.y), -3f, 3f);
    src.spatialBlend = entry.spatial ? 1f : 0f;
    src.minDistance = Mathf.Max(0.01f, entry.minDistance);
    src.maxDistance = Mathf.Max(src.minDistance, entry.maxDistance);
    src.outputAudioMixerGroup = entry.mixerGroup != null ? entry.mixerGroup : defaultMixerGroup;
    src.loop = false;

    src.Play();
    StartCoroutine(ReleaseAfter(src, clip.length / Mathf.Max(0.01f, Mathf.Abs(src.pitch))));
  }

  private IEnumerator ReleaseAfter(AudioSource src, float seconds)
  {
    yield return new WaitForSeconds(seconds);
    if (src != null) src.Stop();
  }
}
