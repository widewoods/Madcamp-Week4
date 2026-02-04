using Unity.Netcode;
using UnityEngine;

public class SfxNetEmitter : NetworkBehaviour
{
  public static SfxNetEmitter Instance { get; private set; }

  public override void OnNetworkSpawn()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
  }

  public override void OnNetworkDespawn()
  {
    if (Instance == this)
      Instance = null;
  }

  public void ServerPlay(SfxId id, Vector3 position, bool spatial = true, float volumeScale = 1f)
  {
    if (!IsServer) return;
    PlayClientRpc(id, position, spatial, volumeScale);
  }

  [Rpc(SendTo.ClientsAndHost)]
  private void PlayClientRpc(SfxId id, Vector3 position, bool spatial, float volumeScale)
  {
    if (SfxManager.Instance == null)
    {
      Debug.LogWarning("[SfxNetEmitter] Missing SfxManager in scene.");
      return;
    }

    if (spatial)
      SfxManager.Instance.Play3D(id, position, volumeScale);
    else
      SfxManager.Instance.Play2D(id, volumeScale);
  }
}
