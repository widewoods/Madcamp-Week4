using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BowlingClearTrigger : NetworkBehaviour
{
  [SerializeField] private BowlingManager bowlingManager;

  void OnTriggerEnter(Collider other)
  {
    if (!IsServer) return;
    if (!other.CompareTag("Minigame")) return;

    var netObj = other.GetComponentInParent<NetworkObject>();
    if (netObj == null) return;

    StartCoroutine(SendServerRegisterShotAfter(3, netObj));

    netObj.Despawn(true);
  }

  IEnumerator SendServerRegisterShotAfter(float seconds, NetworkObject netObj)
  {
    yield return new WaitForSeconds(seconds);
    if (bowlingManager == null)
      bowlingManager = FindFirstObjectByType<BowlingManager>();

    if (bowlingManager != null)
      bowlingManager.ServerRegisterShot(netObj.OwnerClientId);
  }
}
