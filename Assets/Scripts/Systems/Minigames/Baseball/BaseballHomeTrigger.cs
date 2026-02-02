using Unity.Netcode;
using UnityEngine;

public class BaseballHomeTrigger : NetworkBehaviour
{
  [SerializeField] private BaseballManager manager;

  private void OnTriggerEnter(Collider other)
  {
    if (!IsServer) return;
    var netObj = other.GetComponentInParent<NetworkObject>();
    if (netObj == null || !netObj.IsPlayerObject) return;

    if (manager == null)
      manager = FindFirstObjectByType<BaseballManager>();

    if (manager != null)
      manager.ServerSchedulePitch();
  }
}
