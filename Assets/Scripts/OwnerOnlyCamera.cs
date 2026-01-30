using Unity.Netcode;
using UnityEngine;

public class OwnerOnlyCamera : NetworkBehaviour
{
  [SerializeField] Camera playerCamera;
  [SerializeField] AudioListener audioListener;

  public override void OnNetworkSpawn()
  {
    bool local = IsOwner;

    if (playerCamera == null)
      playerCamera = GetComponentInChildren<Camera>(true);

    if (audioListener == null)
      audioListener = GetComponentInChildren<AudioListener>(true);

    if (playerCamera != null)
      playerCamera.gameObject.SetActive(local);

    if (audioListener != null)
      audioListener.enabled = local;
  }
}
