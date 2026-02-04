using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Audio/Sfx Library")]
public class SfxLibrary : ScriptableObject
{
  [System.Serializable]
  public class SfxEntry
  {
    public SfxId id = SfxId.None;
    public AudioClip[] clips;
    [Range(0f, 1f)] public float volume = 1f;
    public Vector2 pitchRange = new Vector2(1f, 1f);
    public bool spatial = true;
    public float minDistance = 1f;
    public float maxDistance = 20f;
    public AudioMixerGroup mixerGroup;
  }

  [SerializeField] private List<SfxEntry> entries = new List<SfxEntry>();
  private Dictionary<SfxId, SfxEntry> lookup;

  private void OnEnable()
  {
    BuildLookup();
  }

  private void BuildLookup()
  {
    lookup = new Dictionary<SfxId, SfxEntry>();
    foreach (var entry in entries)
    {
      if (entry == null) continue;
      if (entry.id == SfxId.None) continue;
      lookup[entry.id] = entry;
    }
  }

  public bool TryGetEntry(SfxId id, out SfxEntry entry)
  {
    if (lookup == null) BuildLookup();
    return lookup.TryGetValue(id, out entry);
  }

  public AudioClip PickClip(SfxEntry entry)
  {
    if (entry == null || entry.clips == null || entry.clips.Length == 0) return null;
    if (entry.clips.Length == 1) return entry.clips[0];
    int index = Random.Range(0, entry.clips.Length);
    return entry.clips[index];
  }
}
