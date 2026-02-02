using Unity.Netcode;
using UnityEngine;

public class PinVisibility : NetworkBehaviour
{
  private readonly NetworkVariable<bool> isVisible = new NetworkVariable<bool>(
    true,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  private Renderer[] renderers;
  private Collider[] colliders;

  private void Awake()
  {
    renderers = GetComponentsInChildren<Renderer>(true);
    colliders = GetComponentsInChildren<Collider>(true);
  }

  public override void OnNetworkSpawn()
  {
    isVisible.OnValueChanged += HandleVisibilityChanged;
    HandleVisibilityChanged(true, isVisible.Value);
  }

  public override void OnNetworkDespawn()
  {
    isVisible.OnValueChanged -= HandleVisibilityChanged;
  }

  public void ServerSetVisible(bool value)
  {
    if (!IsServer) return;
    isVisible.Value = value;
  }

  private void HandleVisibilityChanged(bool previous, bool current)
  {
    if (renderers != null)
    {
      foreach (var r in renderers)
        if (r != null) r.enabled = current;
    }

    if (colliders != null)
    {
      foreach (var c in colliders)
        if (c != null) c.enabled = current;
    }
  }
}
