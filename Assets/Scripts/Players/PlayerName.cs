using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerName : NetworkBehaviour
{
  private readonly NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(
    new FixedString64Bytes("Player"),
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  public string Name => playerName.Value.ToString();

  public override void OnNetworkSpawn()
  {
    playerName.OnValueChanged += HandleNameChanged;
    HandleNameChanged(playerName.Value, playerName.Value);
  }

  public override void OnNetworkDespawn()
  {
    playerName.OnValueChanged -= HandleNameChanged;
  }

  public void RequestSetName(string newName)
  {
    if (!IsOwner) return;
    if (string.IsNullOrWhiteSpace(newName)) return;
    SetNameServerRpc(newName.Trim());
  }

  private void HandleNameChanged(FixedString64Bytes previous, FixedString64Bytes current)
  {
    // Intentionally empty; PlayerNameTag reads Name on change via subscription.
  }

  [Rpc(SendTo.Server)]
  private void SetNameServerRpc(string newName)
  {
    if (string.IsNullOrWhiteSpace(newName)) return;
    if (playerName.Value.ToString() != "Player") return;
    playerName.Value = new FixedString64Bytes(newName.Trim());
  }
}
