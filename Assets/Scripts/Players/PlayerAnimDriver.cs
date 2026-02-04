using Unity.Netcode;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class PlayerAnimDriver : NetworkBehaviour
{
  [SerializeField] Animator animator;
  [SerializeField] BowlingBallRollInput bowlingBallRollInput;
  [SerializeField] GameObject ball;
  [SerializeField] CharacterController cc;
  [SerializeField] GameObject baseballBat;
  [SerializeField] BaseballSwingInput baseballSwingInput;
  [Header("Footsteps")]
  [SerializeField] private AudioSource footstepSource;
  [SerializeField] private AudioClip[] footstepClips;
  [SerializeField] private Vector2 footstepVolumeRange = new Vector2(0.8f, 1f);
  [SerializeField] private Vector2 footstepPitchRange = new Vector2(0.95f, 1.05f);

  void Reset()
  {
    animator = GetComponentInChildren<Animator>();
    cc = GetComponent<CharacterController>();
  }

  private void Awake()
  {
    if (animator == null) animator = GetComponentInChildren<Animator>();
    if (cc == null) cc = GetComponent<CharacterController>();
    EnsureFootstepSource();
  }

  void Update()
  {
    if (!IsOwner) return;
    if (animator == null || cc == null) return;

    Vector3 v = cc.velocity;
    v.y = 0f;

    animator.SetFloat("Speed", v.magnitude);
  }

  public void BowlingTrigger()
  {
    bowlingBallRollInput.bowlingThrown = true;
    ball.SetActive(false);

    animator.SetTrigger("Idle");
  }

  public void Idle()
  {
    animator.SetTrigger("Idle");
  }

  public void OnBaseballAnimationFinished()
  {
    if (baseballBat != null)
      baseballBat.SetActive(false);
    baseballSwingInput.OnBaseballAnimationFinished();

  }

  public void OnBaseballSwing()
  {
    baseballSwingInput.AnimationTimingSwing();
  }

  // Animation event
  public void Footstep()
  {
    if (footstepClips == null || footstepClips.Length == 0) return;
    EnsureFootstepSource();
    if (footstepSource == null) return;

    var clip = footstepClips[Random.Range(0, footstepClips.Length)];
    if (clip == null) return;

    float volume = Random.Range(footstepVolumeRange.x, footstepVolumeRange.y);
    float pitch = Random.Range(footstepPitchRange.x, footstepPitchRange.y);
    footstepSource.pitch = Mathf.Clamp(pitch, -3f, 3f);
    footstepSource.PlayOneShot(clip, Mathf.Clamp01(volume));
  }

  private void EnsureFootstepSource()
  {
    if (footstepSource != null) return;
    footstepSource = GetComponent<AudioSource>();
    if (footstepSource == null)
    {
      footstepSource = gameObject.AddComponent<AudioSource>();
      footstepSource.playOnAwake = false;
      footstepSource.loop = false;
      footstepSource.spatialBlend = 1f;
    }
  }
}
