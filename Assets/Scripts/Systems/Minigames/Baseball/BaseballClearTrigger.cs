using Unity.Netcode;
using UnityEngine;

public class BaseballClearTrigger : NetworkBehaviour
{

  void OnTriggerEnter(Collider other)
  {
    if (!IsServer) return;
    if (!other.CompareTag("Minigame")) return;

    var netObj = other.GetComponentInParent<NetworkObject>();
    if (netObj == null) return;

    netObj.Despawn(true);
  }
}
