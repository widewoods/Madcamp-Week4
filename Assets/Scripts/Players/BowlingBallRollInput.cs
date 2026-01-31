using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class BowlingBallRollInput : NetworkBehaviour
{
  [SerializeField] private BowlingBallRoller ballPrefab;
  [SerializeField] private Transform aimSource;
  [SerializeField] private float spawnDistance = 1.2f;

  private void Awake()
  {
    if (aimSource == null) aimSource = transform;
  }

  private void Update()
  {
    if (!IsOwner) return;
    if (Keyboard.current == null) return;
    if (!Keyboard.current.fKey.wasPressedThisFrame) return;
    RequestSpawnAndRollServerRpc(aimSource.position, aimSource.forward);
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
}
