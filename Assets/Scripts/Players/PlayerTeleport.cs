using Unity.Netcode;
using UnityEngine;

public class PlayerTeleport : NetworkBehaviour
{
  [Rpc(SendTo.ClientsAndHost)]
  private void TeleportClientRpc(Vector3 position, Quaternion rotation)
  {
    var controller = GetComponent<CharacterController>();
    if (controller != null) controller.enabled = false;

    transform.SetPositionAndRotation(position, rotation);

    if (controller != null) controller.enabled = true;
  }

  public void ServerTeleport(Vector3 position, Quaternion rotation)
  {
    if (!IsServer) return;
    TeleportClientRpc(position, rotation);
  }
}
