using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GolfStrokeInput : NetworkBehaviour
{
  [Header("Refs")]
  [SerializeField] private GolfBall ball;
  [SerializeField] private GolfManager manager;
  [SerializeField] private Transform aimSource;

  [Header("Power")]
  [SerializeField] private float minForce = 2f;
  [SerializeField] private float maxForce = 12f;
  [SerializeField] private float chargeSpeed = 6f;

  private float charge;

  private void Awake()
  {
    if (aimSource == null) aimSource = transform;
  }

  private void Update()
  {
    if (!IsOwner) return;
    if (Keyboard.current == null) return;
    if (ball == null) return;
    if (ball.IsMoving()) return;

    if (Keyboard.current.fKey.isPressed)
    {
      charge = Mathf.Clamp(charge + chargeSpeed * Time.deltaTime, 0f, 1f);
      return;
    }

    if (Keyboard.current.fKey.wasReleasedThisFrame)
    {
      float force = Mathf.Lerp(minForce, maxForce, charge);
      charge = 0f;
      RequestStrokeServerRpc(aimSource.forward, force);
    }
  }

  [Rpc(SendTo.Server)]
  private void RequestStrokeServerRpc(Vector3 forward, float force, RpcParams rpcParams = default)
  {
    if (ball == null) return;
    if (manager == null)
      manager = FindFirstObjectByType<GolfManager>();

    ball.ServerStroke(forward, force);
    if (manager != null)
      manager.ServerRegisterStroke(rpcParams.Receive.SenderClientId);
  }
}
