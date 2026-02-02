using Unity.Netcode;
using UnityEngine;

public class GolfHoleTrigger : NetworkBehaviour
{
  [SerializeField] private GolfManager manager;
  [SerializeField] private Transform resetPoint;

  private void OnTriggerEnter(Collider other)
  {
    if (!IsServer) return;
    var ball = other.GetComponentInParent<GolfBall>();
    if (ball == null) return;

    if (resetPoint != null)
    {
      var rb = ball.GetComponent<Rigidbody>();
      if (rb != null)
      {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
      }
      ball.transform.position = resetPoint.position;
    }

    if (manager == null)
      manager = FindFirstObjectByType<GolfManager>();

    if (manager != null)
      manager.ServerResetCourse();
  }
}
