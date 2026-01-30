using Unity.Netcode;
using UnityEngine;

public class SeatInteractable : InteractableBase
{
  [Header("Seat")]
  [SerializeField] private int seatId = -1;
  [SerializeField] private Transform seatPoint;
  [SerializeField] private Transform exitPoint;
  [SerializeField] private float yawOffset = 0f;
  [SerializeField] private ClassroomManager classroomManager;

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

    var manager = GetManagerServer();
    if (manager == null) return;

    var playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
    if (playerObj == null) return;

    var teleport = playerObj.GetComponent<PlayerTeleport>();
    if (teleport == null) return;

    var seating = playerObj.GetComponent<PlayerSeating>();
    if (seating == null) return;

    var anim = playerObj.gameObject.GetComponentInChildren<Animator>(true);
    if (anim == null) Debug.Log("Cannot find animator");

    if (IsOccupied)
    {
      if (clientId != occupiedBy.Value) return;

      occupiedBy.Value = ulong.MaxValue;

      var target = exitPoint != null ? exitPoint : transform;
      var rotation = target.rotation * Quaternion.Euler(0f, yawOffset, 0f);

      teleport.ServerTeleport(target.position, rotation);
      anim.SetTrigger("Idle");
      seating.ServerSetSeated(false);
      manager.ServerSeatVacated(seatId, clientId);
      return;
    }

    occupiedBy.Value = clientId;

    var seatTarget = seatPoint != null ? seatPoint : transform;
    var seatRotation = seatTarget.rotation * Quaternion.Euler(0f, yawOffset, 0f);

    teleport.ServerTeleport(seatTarget.position, seatRotation);
    anim.SetTrigger("Seating");
    seating.ServerSetSeated(true);
    manager.ServerSeatOccupied(seatId, clientId);
  }

  public override void OnNetworkSpawn()
  {
    if (!IsServer) return;
    var manager = GetManagerServer();
    if (manager == null) return;
    manager.ServerRegisterSeat(seatId);
  }

  private ClassroomManager GetManagerServer()
  {
    if (classroomManager == null)
      classroomManager = FindFirstObjectByType<ClassroomManager>();
    return classroomManager;
  }
}
