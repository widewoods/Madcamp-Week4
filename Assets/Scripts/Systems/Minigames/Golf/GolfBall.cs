using Unity.Netcode;
using UnityEngine;

public class GolfBall : NetworkBehaviour
{
  [SerializeField] private Rigidbody rb;
  [SerializeField] private float stopSpeed = 0.05f;

  private void Awake()
  {
    if (rb == null) rb = GetComponent<Rigidbody>();
  }

  public bool IsMoving()
  {
    if (rb == null) return false;
    return rb.linearVelocity.magnitude > stopSpeed;
  }

  public void ServerStroke(Vector3 direction, float force)
  {
    if (!IsServer) return;
    if (rb == null) return;

    direction.y = 0f;
    if (direction.sqrMagnitude < 0.001f) return;
    direction.Normalize();

    rb.linearVelocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;
    rb.AddForce(direction * force, ForceMode.VelocityChange);
  }
}
