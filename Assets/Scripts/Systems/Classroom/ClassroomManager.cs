using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClassroomManager : NetworkBehaviour
{
  public enum StartMode
  {
    AutoOnAllSeated,
    HostStarts,
    Hybrid
  }

  [Header("Start Rules")]
  [SerializeField] private StartMode startMode = StartMode.AutoOnAllSeated;
  [SerializeField] private int requiredSeats = 0;
  [SerializeField] private bool allowUnseatAfterStart = true;
  [SerializeField] private int totalSeats = 21;
  [SerializeField] private bool requireAllSeatedForStart = true;

  [Header("Start Targets")]
  [SerializeField] private DoorInteractable[] doorsToOpen;

  private readonly NetworkVariable<int> seatedCount = new NetworkVariable<int>(
    0,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  private readonly NetworkVariable<bool> hasStarted = new NetworkVariable<bool>(
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  public event Action<int, int> OnSeatedChanged;
  public event Action<bool> OnStartedChanged;

  public int SeatedCount => seatedCount.Value;
  public bool HasStarted => hasStarted.Value;
  public bool AllowUnseatAfterStart => allowUnseatAfterStart;
  public int TotalSeats => totalSeats;

  private readonly Dictionary<int, ulong> seatToClient = new Dictionary<int, ulong>();
  private readonly HashSet<ulong> seatedClients = new HashSet<ulong>();
  private readonly HashSet<int> registeredSeats = new HashSet<int>();

  public override void OnNetworkSpawn()
  {
    seatedCount.OnValueChanged += HandleSeatedCountChanged;
    hasStarted.OnValueChanged += HandleStartedChanged;

    HandleSeatedCountChanged(0, seatedCount.Value);
    HandleStartedChanged(false, hasStarted.Value);
  }

  public override void OnNetworkDespawn()
  {
    seatedCount.OnValueChanged -= HandleSeatedCountChanged;
    hasStarted.OnValueChanged -= HandleStartedChanged;
  }

  public void ServerSeatChanged(int delta)
  {
    if (!IsServer) return;
    if (hasStarted.Value && !allowUnseatAfterStart && delta < 0) return;

    int next = Mathf.Max(0, seatedCount.Value + delta);
    seatedCount.Value = next;

    if (ShouldAutoStartServer())
      ServerStart();
  }

  public void ServerRegisterSeat(int seatId)
  {
    if (!IsServer) return;
    if (seatId < 0) return;
    if (!registeredSeats.Add(seatId)) return;
    if (totalSeats > 0 && registeredSeats.Count > totalSeats)
      Debug.LogWarning($"Registered seats exceeded totalSeats ({totalSeats}).");
  }

  public void ServerSeatOccupied(int seatId, ulong clientId)
  {
    if (!IsServer) return;
    if (seatId < 0) return;
    if (!registeredSeats.Contains(seatId)) registeredSeats.Add(seatId);

    seatToClient[seatId] = clientId;
    seatedClients.Add(clientId);
    UpdateSeatedCountServer();
  }

  public void ServerSeatVacated(int seatId, ulong clientId)
  {
    if (!IsServer) return;
    if (seatId < 0) return;

    if (seatToClient.TryGetValue(seatId, out var occupant))
    {
      if (occupant != clientId) return;
      seatToClient.Remove(seatId);
    }

    bool stillSeated = false;
    foreach (var kvp in seatToClient)
    {
      if (kvp.Value == clientId)
      {
        stillSeated = true;
        break;
      }
    }

    if (!stillSeated)
      seatedClients.Remove(clientId);

    UpdateSeatedCountServer();
  }

  [Rpc(SendTo.Server)]
  public void RequestStartServerRpc(RpcParams rpcParams = default)
  {
    if (!IsServer) return;

    if (startMode == StartMode.AutoOnAllSeated)
      return;

    ulong senderId = rpcParams.Receive.SenderClientId;
    if (senderId != NetworkManager.ServerClientId)
      return;

    int required = GetRequiredSeatsServer();
    if (required > 0 && seatedCount.Value == 0)
      return;

    if (requireAllSeatedForStart && required > 0 && seatedCount.Value < required)
      return;

    ServerStart();
  }

  public int GetRequiredSeatsServer()
  {
    if (!IsServer) return requiredSeats;
    if (requiredSeats > 0) return requiredSeats;
    return NetworkManager.Singleton != null ? NetworkManager.Singleton.ConnectedClientsIds.Count : 0;
  }

  private bool ShouldAutoStartServer()
  {
    if (!IsServer) return false;
    if (hasStarted.Value) return false;
    if (startMode == StartMode.HostStarts) return false;

    int required = GetRequiredSeatsServer();
    if (requireAllSeatedForStart)
      return required > 0 && seatedCount.Value >= required;
    return seatedCount.Value > 0;
  }

  private void ServerStart()
  {
    if (!IsServer) return;
    if (hasStarted.Value) return;
    Debug.Log("ServerStart");
    hasStarted.Value = true;
    OpenDoorsServer();
  }

  private void OpenDoorsServer()
  {
    if (!IsServer) return;
    if (doorsToOpen == null) return;
    foreach (var door in doorsToOpen)
    {
      if (door == null) continue;
      door.ServerOpen();
    }
  }

  private void UpdateSeatedCountServer()
  {
    if (!IsServer) return;
    seatedCount.Value = Mathf.Max(0, seatedClients.Count);
    if (ShouldAutoStartServer())
      ServerStart();
  }

  private void HandleSeatedCountChanged(int previous, int current)
  {
    int required = requiredSeats;
    if (NetworkManager.Singleton != null)
    {
      bool shouldUseDynamic = requiredSeats <= 0;
      if (shouldUseDynamic)
        required = NetworkManager.Singleton.ConnectedClientsIds.Count;
    }

    OnSeatedChanged?.Invoke(current, required);
  }

  private void HandleStartedChanged(bool previous, bool current)
  {
    OnStartedChanged?.Invoke(current);
  }
}
