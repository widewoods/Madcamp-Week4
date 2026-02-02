using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BowlingBallRollInput : NetworkBehaviour
{
  [SerializeField] private BowlingBallRoller ballPrefab;
  [SerializeField] private Transform aimSource;
  [SerializeField] GameObject ball;
  [SerializeField] private Animator animator;
  [SerializeField] private float spawnDistance = 1.2f;

  public bool bowlingThrown = false;

  private void Awake()
  {
    if (aimSource == null) aimSource = transform;
    if (animator == null) animator = GetComponentInChildren<Animator>();
  }

  private void Update()
  {
    if (!IsOwner) return;
    if (Keyboard.current == null) return;
    if (!Keyboard.current.fKey.wasPressedThisFrame) return;
    StartCoroutine(StartBowlingAnimation(aimSource.position, aimSource.forward));
  }

  [Rpc(SendTo.Server)]
  private void RequestSpawnAndRollServerRpc(Vector3 origin, Vector3 forward, RpcParams rpcParams = default)
  {
    if (!IsServer) return;
    if (ballPrefab == null) return;

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
    playerMovement.enabled = false;
    cameraMovement.enabled = false;

    yield return new WaitUntil(() => bowlingThrown);

    playerMovement.enabled = true;
    cameraMovement.enabled = true;

    RequestSpawnAndRollServerRpc(origin, forward);
  }

}
