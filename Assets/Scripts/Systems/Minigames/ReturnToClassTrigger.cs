using Unity.Netcode;
using UnityEngine;

public class ReturnToClassTrigger : NetworkBehaviour
{
  [SerializeField] private Transform teleportTransform;

  void OnTriggerEnter(Collider other)
  {
    if (!IsServer) return;
    if (teleportTransform == null)
    {
      Debug.Log("[Trigger] Teleport Transform is null");
      return;
    }

    NetworkObject netObj = other.GetComponentInParent<NetworkObject>();
    if (netObj == null)
    {
      Debug.Log("[Trigger] Network object not found");
      return;
    }
    if (!netObj.IsPlayerObject) return;

    PlayerTeleport playerTeleport = other.GetComponentInParent<PlayerTeleport>();
    if (playerTeleport == null)
    {
      Debug.Log("[Trigger] Player Teleport not found");
      return;
    }

    playerTeleport.ServerTeleport(teleportTransform.position, other.transform.rotation);
  }
}
