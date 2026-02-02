using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerName : NetworkBehaviour
{
  public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(
    new FixedString64Bytes("Player"),
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  public string Name => playerName.Value.ToString();

  public void RequestSetName(string newName)
  {
    if (!IsOwner) return;
    if (string.IsNullOrWhiteSpace(newName)) return;
    SetNameServerRpc(newName.Trim());
  }


  [Rpc(SendTo.Server)]
  private void SetNameServerRpc(string newName)
  {
    if (string.IsNullOrWhiteSpace(newName)) return;
    if (playerName.Value.ToString() != "Player") return;
    playerName.Value = new FixedString64Bytes(newName.Trim());
  }
}
