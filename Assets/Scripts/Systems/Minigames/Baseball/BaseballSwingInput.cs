using Unity.Netcode;
using UnityEngine;

public class BaseballSwingInput : NetworkBehaviour, IMinigameUseHandler
{
  [SerializeField] private BaseballManager manager;
  [SerializeField] private Transform aimSource;
  [SerializeField] private Animator animator;
  [SerializeField] private GameObject baseballObject;
  [SerializeField] private string baseballTrigger = "Baseball";
  [SerializeField] private NGOPlayerMovement playerMovement;

  private bool movementLocked;

  private void Awake()
  {
    if (aimSource == null) aimSource = transform;
    if (animator == null) animator = GetComponentInChildren<Animator>();
    if (baseballObject != null) baseballObject.SetActive(false);
    if (playerMovement == null) playerMovement = GetComponent<NGOPlayerMovement>();
  }


  public MinigameType MinigameType => MinigameType.Baseball;

  public void OnUsePressed()
  {
    if (!IsOwner) return;
    PlayBaseballAnimation();
  }

  public void AnimationTimingSwing()
  {
    RequestSwingServerRpc(aimSource.position);
  }

  public void OnUseHeld() { }
  public void OnUseReleased() { }

  [Rpc(SendTo.Server)]
  private void RequestSwingServerRpc(Vector3 forward, RpcParams rpcParams = default)
  {
    if (manager == null)
      manager = FindFirstObjectByType<BaseballManager>();
    if (manager == null) return;
    manager.ServerTrySwing(rpcParams.Receive.SenderClientId, forward);
  }

  private void PlayBaseballAnimation()
  {
    LockMovement(true);
    if (baseballObject != null)
      baseballObject.SetActive(true);
    if (animator != null && !string.IsNullOrWhiteSpace(baseballTrigger))
      animator.SetTrigger(baseballTrigger);
  }

  // Hook this to an animation event at the end of the swing clip.
  public void OnBaseballAnimationFinished()
  {
    if (!IsOwner) return;
    LockMovement(false);
  }

  private void LockMovement(bool locked)
  {
    if (movementLocked == locked) return;
    movementLocked = locked;
    if (playerMovement != null)
      playerMovement.enabled = !locked;
  }
}
