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
  [SerializeField] GolfStrokeInput golfStrokeInput;
  [SerializeField] GameObject golfClub;


  void Reset()
  {
    animator = GetComponentInChildren<Animator>();
    cc = GetComponent<CharacterController>();
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
    golfClub.SetActive(false);
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
    SfxManager.Instance?.Play3D(SfxId.GolfSwing, transform.position, 0.6f);
  }

  public void OnGolfStart()
  {
    golfClub.SetActive(true);
  }

  public void OnGolfSwing()
  {
    if (golfStrokeInput != null)
      golfStrokeInput.AnimationTimingStroke();
  }
}
