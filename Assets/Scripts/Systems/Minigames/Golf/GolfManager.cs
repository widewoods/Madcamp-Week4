using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GolfManager : NetworkBehaviour
{
  [Header("Course")]
  [SerializeField] private Transform teePoint;
  [SerializeField] private Transform holePoint;

  private readonly NetworkVariable<int> totalStrokes = new NetworkVariable<int>(
    0,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  private readonly NetworkVariable<ulong> lastShooterId = new NetworkVariable<ulong>(
    ulong.MaxValue,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  private readonly Dictionary<ulong, int> strokesByPlayer = new Dictionary<ulong, int>();

  public int TotalStrokes => totalStrokes.Value;
  public ulong LastShooterId => lastShooterId.Value;

  public override void OnNetworkSpawn()
  {
    if (!IsServer) return;
  }

  public Vector3 GetTeePosition()
  {
    return teePoint != null ? teePoint.position : Vector3.zero;
  }

  public void ServerRegisterStroke(ulong clientId)
  {
    if (!IsServer) return;
    lastShooterId.Value = clientId;

    if (!strokesByPlayer.ContainsKey(clientId))
      strokesByPlayer[clientId] = 0;

    strokesByPlayer[clientId] += 1;
    totalStrokes.Value += 1;
  }

  public int GetStrokesForPlayer(ulong clientId)
  {
    if (!strokesByPlayer.TryGetValue(clientId, out var value))
      return 0;
    return value;
  }

  public void ServerResetCourse()
  {
    if (!IsServer) return;
    strokesByPlayer.Clear();
    totalStrokes.Value = 0;
    lastShooterId.Value = ulong.MaxValue;
  }
}
