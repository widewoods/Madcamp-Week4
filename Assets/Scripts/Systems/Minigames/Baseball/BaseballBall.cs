using System;
using Unity.Netcode;
using UnityEngine;

public class BaseballBall : NetworkBehaviour
{
  [SerializeField] private Rigidbody rb;
  [SerializeField] private float stopSpeed = 0.2f;
  [SerializeField] private float stopConfirmTime = 0.5f;

  private Vector3 hitStartPosition;
  private bool inPlay;
  private float stopTimer;

  public bool InPlay => inPlay;
  public Vector3 HitStartPosition => hitStartPosition;


  private void Awake()
  {
    if (rb == null) rb = GetComponent<Rigidbody>();
  }

  public void ServerLaunch(Vector3 velocity)
  {
    if (!IsServer) return;
    if (rb == null) return;
    inPlay = false;
    stopTimer = 0f;
    rb.linearVelocity = velocity;
    rb.angularVelocity = Vector3.zero;
  }

  public void ServerHit(Vector3 direction, float force)
  {
    if (!IsServer) return;
    if (rb == null) return;
    stopTimer = 0f;

    direction.y = 0f;
    if (direction.sqrMagnitude < 0.001f) return;
    direction.Normalize();

    rb.linearVelocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;
    rb.AddForce(direction * force, ForceMode.VelocityChange);

    hitStartPosition = transform.position;
    inPlay = true;
  }

  public bool IsStopped()
  {
    if (rb == null) return true;

    if (rb.linearVelocity.magnitude <= stopSpeed)
      stopTimer += Time.deltaTime;
    else
      stopTimer = 0f;

    if (stopTimer >= stopConfirmTime)
    {
      inPlay = false;
      stopTimer = 0f;
      return true;
    }
    return false;
  }
}
