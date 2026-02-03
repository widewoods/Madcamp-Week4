using Unity.Netcode;
using UnityEngine;

public class BaseballSwingInput : NetworkBehaviour, IMinigameUseHandler
{
  [SerializeField] private BaseballManager manager;
  [SerializeField] private Transform aimSource;

  private void Awake()
  {
    if (aimSource == null) aimSource = transform;
  }

  public MinigameType MinigameType => MinigameType.Baseball;

  public void OnUsePressed()
  {
    if (!IsOwner) return;
    RequestSwingServerRpc(aimSource.forward);
  }

  public void OnUseHeld() { }
  public void OnUseReleased() { }

  [Rpc(SendTo.Server)]
  private void RequestSwingServerRpc(Vector3 forward, RpcParams rpcParams = default)
  {
    if (manager == null)
      manager = FindFirstObjectByType<BaseballManager>();
    if (manager == null) return;
    manager.ServerTrySwing(rpcParams.Receive.SenderClientId, forward);
  }
}
