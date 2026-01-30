using Unity.Netcode;
using UnityEngine;

public class PlayerSeating : NetworkBehaviour
{
  private readonly NetworkVariable<bool> isSeated = new NetworkVariable<bool>(
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  public bool IsSeated => isSeated.Value;

  public override void OnNetworkSpawn()
  {
    if (!IsOwner) return;
    isSeated.OnValueChanged += OnSeatedChanged;
    OnSeatedChanged(false, isSeated.Value);
  }

  public override void OnNetworkDespawn()
  {
    if (!IsOwner) return;
    isSeated.OnValueChanged -= OnSeatedChanged;
  }

  public void ServerSetSeated(bool seated)
  {
    if (!IsServer) return;
    isSeated.Value = seated;
  }

  private void OnSeatedChanged(bool previous, bool current)
  {
    var movement = GetComponent<NGOPlayerMovement>();
    if (movement != null) movement.enabled = !current;

    var controller = GetComponent<CharacterController>();
    if (controller != null) controller.enabled = !current;
  }
}
