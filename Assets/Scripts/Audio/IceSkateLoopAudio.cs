using UnityEngine;

public class IceSkateLoopAudio : MonoBehaviour
{
  [Header("Refs")]
  [SerializeField] private NGOPlayerMovement movement;
  [SerializeField] private AudioSource audioSource;

  [Header("Loop")]
  [SerializeField] private AudioClip loopClip;
  [SerializeField] private float minSpeed = 0.2f;
  [SerializeField] private float fadeInSeconds = 0.15f;
  [SerializeField] private float fadeOutSeconds = 0.2f;
  [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

  private float targetVolume;
  private bool wasPlaying;

  private void Awake()
  {
    if (movement == null) movement = GetComponentInParent<NGOPlayerMovement>();
    if (audioSource == null)
      audioSource = GetComponent<AudioSource>();

    if (audioSource == null)
    {
      audioSource = gameObject.AddComponent<AudioSource>();
      audioSource.playOnAwake = false;
      audioSource.loop = true;
      audioSource.spatialBlend = 1f;
      audioSource.volume = 0f;
    }

    if (loopClip != null)
      audioSource.clip = loopClip;
  }

  private void Update()
  {
    if (movement == null || audioSource == null) return;
    if (!movement.IsOwner) return;

    bool shouldPlay = movement.IsOnIce && movement.HorizontalSpeed >= minSpeed && loopClip != null;

    if (shouldPlay && !audioSource.isPlaying)
    {
      audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
      audioSource.Play();
    }

    targetVolume = shouldPlay ? 1f : 0f;
    float fade = targetVolume > audioSource.volume ? fadeInSeconds : fadeOutSeconds;
    if (fade <= 0f)
    {
      audioSource.volume = targetVolume;
    }
    else
    {
      audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, Time.deltaTime / fade);
    }

    if (!shouldPlay && audioSource.isPlaying && audioSource.volume <= 0.001f)
      audioSource.Stop();

    wasPlaying = shouldPlay;
  }
}
