using Unity.Netcode;
using UnityEngine;

public class BowlingBallRoller : NetworkBehaviour
{
  [Header("Roll")]
  [SerializeField] private float rollForce = 12f;
  [SerializeField] private float upwardForce = 0f;

  [Header("Refs")]
  [SerializeField] private Rigidbody rb;

  private void Awake()
  {
    if (rb == null) rb = GetComponent<Rigidbody>();
  }

  public void RollFrom(Transform source)
  {
    if (source == null) return;
    if (!IsOwner) return;
    RequestRollServerRpc(source.forward);
  }

  public void ServerRoll(Vector3 forward)
  {
    if (!IsServer) return;
    ApplyRoll(forward);
  }

  [Rpc(SendTo.Server)]
  private void RequestRollServerRpc(Vector3 forward, RpcParams rpcParams = default)
  {
    if (!IsServer) return;
    ApplyRoll(forward);
  }

  private void ApplyRoll(Vector3 forward)
  {
    if (rb == null) return;

    forward.y = 0f;
    if (forward.sqrMagnitude < 0.001f) return;
    forward.Normalize();

    rb.linearVelocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;

    Vector3 force = forward * rollForce + Vector3.up * upwardForce;
    rb.AddForce(force, ForceMode.VelocityChange);
  }
}
