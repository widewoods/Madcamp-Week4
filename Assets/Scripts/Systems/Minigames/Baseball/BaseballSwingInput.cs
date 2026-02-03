using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class BaseballSwingInput : NetworkBehaviour
{
  [SerializeField] private BaseballManager manager;
  [SerializeField] private Transform aimSource;

  private void Awake()
  {
    if (aimSource == null) aimSource = transform;
  }

  private void Update()
  {
    if (!IsOwner) return;
    if (Keyboard.current == null) return;
    if (!Keyboard.current.fKey.wasPressedThisFrame) return;
    RequestSwingServerRpc(aimSource.position);
  }

  [Rpc(SendTo.Server)]
  private void RequestSwingServerRpc(Vector3 position, RpcParams rpcParams = default)
  {
    if (manager == null)
      manager = FindFirstObjectByType<BaseballManager>();
    if (manager == null) return;
    manager.ServerTrySwing(rpcParams.Receive.SenderClientId, position);
  }
}
