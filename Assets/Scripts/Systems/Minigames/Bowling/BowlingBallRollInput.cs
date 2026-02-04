using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BowlingBallRollInput : NetworkBehaviour, IMinigameUseHandler
{
  [SerializeField] private BowlingBallRoller ballPrefab;
  [SerializeField] private Transform aimSource;
  [SerializeField] GameObject ball;
  [SerializeField] private Animator animator;
  [SerializeField] private float spawnDistance = 1.2f;
  [SerializeField] private float moveDistance = 0.8f;
  [SerializeField] private float moveDuration = 0.45f;
  [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

  public bool bowlingThrown = false;

  private void Awake()
  {
    if (aimSource == null) aimSource = transform;
    if (animator == null) animator = GetComponentInChildren<Animator>();
  }

  public MinigameType MinigameType => MinigameType.Bowling;

  public void OnUsePressed()
  {
    if (!IsOwner) return;
    StartCoroutine(StartBowlingAnimation(aimSource.position, aimSource.forward));
  }

  public void OnUseHeld() { }
  public void OnUseReleased() { }

  [Rpc(SendTo.Server)]
  private void RequestSpawnAndRollServerRpc(Vector3 origin, Vector3 forward, RpcParams rpcParams = default)
  {
    if (!IsServer) return;
    if (ballPrefab == null) return;

    SfxNetEmitter.Instance?.ServerPlay(SfxId.BowlingBallThrow, origin);

    forward.y = 0f;
    if (forward.sqrMagnitude < 0.001f) return;
    forward.Normalize();

    Vector3 spawnPos = origin + forward * spawnDistance;
    var ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity);

    var netObj = ball.GetComponent<NetworkObject>();
    if (netObj == null) return;

    ulong ownerId = rpcParams.Receive.SenderClientId;
    netObj.SpawnWithOwnership(ownerId);

    ball.ServerRoll(forward);
  }

  IEnumerator StartBowlingAnimation(Vector3 origin, Vector3 forward)
  {
    ball.SetActive(true);
    bowlingThrown = false;
    animator.SetTrigger("Bowling");
    var playerMovement = GetComponent<NGOPlayerMovement>();
    var cameraMovement = GetComponent<NGOMouseLookInputSystem>();
    var controller = GetComponent<CharacterController>();
    playerMovement.enabled = false;
    cameraMovement.enabled = false;

    Vector3 flatForward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
    Vector3 startPos = transform.position;
    Vector3 targetPos = startPos + flatForward * moveDistance;
    float elapsed = 0f;

    while (elapsed < moveDuration)
    {
      elapsed += Time.deltaTime;
      float t = Mathf.Clamp01(elapsed / moveDuration);
      float eased = moveCurve.Evaluate(t);
      Vector3 nextPos = Vector3.Lerp(startPos, targetPos, eased);

      if (controller != null)
        controller.Move(nextPos - transform.position);
      else
        transform.position = nextPos;

      yield return null;
    }

    yield return new WaitUntil(() => bowlingThrown);

    playerMovement.enabled = true;
    cameraMovement.enabled = true;

    RequestSpawnAndRollServerRpc(origin, forward);
  }

}
