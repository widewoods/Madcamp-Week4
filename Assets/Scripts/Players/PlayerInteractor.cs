using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : NetworkBehaviour
{
  [Header("Refs")]
  [SerializeField] private Camera playerCamera;
  [SerializeField] private PlayerInput playerInput;

  private InputAction interactAction;

  [Header("Raycaset")]
  [SerializeField] private float interactRange = 3f;
  [SerializeField] private LayerMask interactMask;

  [Header("Debug")]
  [SerializeField] private bool drawRay = true;

  private InteractableBase current;
  public event Action<InteractableBase> OnTargetChanged;

  public override void OnNetworkSpawn()
  {
    if (playerInput == null) playerInput = GetComponent<PlayerInput>();

    if (!IsOwner)
    {
      playerInput.enabled = false;
      enabled = false;
      return;
    }

    if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();

    interactAction = playerInput.actions["Interact"];
    interactAction.Enable();
  }

  private void OnDisable()
  {
    interactAction?.Disable();
  }

  void Update()
  {
    if (!IsOwner) return;

    var hit = RaycastForInteractable();

    if (hit != current)
    {
      // TODO: 화면에 "E - {current.Prompt}" 띄우기
      current = hit;
      OnTargetChanged?.Invoke(current);
    }

    if (current != null && interactAction != null && interactAction.WasPressedThisFrame())
    {
      ulong targetId = current.NetworkObjectId;
      RequestInteractServerRpc(targetId);
    }
  }

  private InteractableBase RaycastForInteractable()
  {
    if (playerCamera == null) return null;

    Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

    if (drawRay)
      Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.yellow);

    if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask, QueryTriggerInteraction.Ignore))
    {
      return hit.collider.GetComponentInParent<InteractableBase>();
    }

    return null;
  }

  [Rpc(SendTo.Server)]
  private void RequestInteractServerRpc(ulong targetNetworkObjectId, RpcParams rpcParams = default)
  {
    if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out var netObj))
      return;

    var interactable = netObj.GetComponent<InteractableBase>();
    if (interactable == null)
      return;

    interactable.ServerInteract(rpcParams.Receive.SenderClientId);
  }
}
