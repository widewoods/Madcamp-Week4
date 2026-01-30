using Unity.Netcode;
using UnityEngine;

public class SeatInteractable : InteractableBase
{
  [Header("Seat")]
  [SerializeField] private Transform seatPoint;
  [SerializeField] private Transform exitPoint;
  [SerializeField] private float yawOffset = 0f;

  private readonly NetworkVariable<ulong> occupiedBy = new NetworkVariable<ulong>(
    ulong.MaxValue,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  public bool IsOccupied => occupiedBy.Value != ulong.MaxValue;

  public override string Prompt
  {
    get
    {
      if (!IsOccupied) return "Sit";
      if (NetworkManager.Singleton == null) return "Occupied";
      return occupiedBy.Value == NetworkManager.Singleton.LocalClientId ? "Stand" : "Occupied";
    }
  }

  public override void ServerInteract(ulong clientId)
  {
    if (!IsServer) return;
    var playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
    if (playerObj == null) return;

    var teleport = playerObj.GetComponent<PlayerTeleport>();
    if (teleport == null) return;

    var seating = playerObj.GetComponent<PlayerSeating>();
    if (seating == null) return;

    if (IsOccupied)
    {
      if (clientId != occupiedBy.Value) return;

      occupiedBy.Value = ulong.MaxValue;

      var target = exitPoint != null ? exitPoint : transform;
      var rotation = target.rotation * Quaternion.Euler(0f, yawOffset, 0f);

      teleport.ServerTeleport(target.position, rotation);
      seating.ServerSetSeated(false);
      return;
    }

    occupiedBy.Value = clientId;

    var seatTarget = seatPoint != null ? seatPoint : transform;
    var seatRotation = seatTarget.rotation * Quaternion.Euler(0f, yawOffset, 0f);

    teleport.ServerTeleport(seatTarget.position, seatRotation);
    seating.ServerSetSeated(true);
  }
}
