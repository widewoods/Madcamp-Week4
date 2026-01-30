using Unity.Netcode;
using UnityEngine;

public class DoorInteractable : InteractableBase
{
  [Header("Door")]
  [SerializeField] private Transform doorPivot;
  [SerializeField] private Vector3 openLocalEuler = new Vector3(0f, 90f, 0f);
  [SerializeField] private bool disableColliderOnOpen = true;
  [SerializeField] private Collider doorCollider;

  private readonly NetworkVariable<bool> isOpen = new NetworkVariable<bool>(
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
  );

  private Quaternion closedRotation;

  private void Awake()
  {
    if (doorPivot == null) doorPivot = transform;
    closedRotation = doorPivot.localRotation;
    if (doorCollider == null) doorCollider = GetComponent<Collider>();
  }

  public override void OnNetworkSpawn()
  {
    isOpen.OnValueChanged += HandleOpenChanged;
    HandleOpenChanged(false, isOpen.Value);
  }

  public override void OnNetworkDespawn()
  {
    isOpen.OnValueChanged -= HandleOpenChanged;
  }

  public override void ServerInteract(ulong clientId)
  {
    if (!IsServer) return;
    if (isOpen.Value) return;
    ServerOpen();
  }

  public void ServerOpen()
  {
    if (!IsServer) return;
    if (isOpen.Value) return;
    isOpen.Value = true;
  }

  private void HandleOpenChanged(bool previous, bool current)
  {
    ApplyDoorState(current);
  }

  private void ApplyDoorState(bool open)
  {
    if (doorPivot == null) return;
    if (open)
      doorPivot.localRotation = closedRotation * Quaternion.Euler(openLocalEuler);
    else
      doorPivot.localRotation = closedRotation;

    if (disableColliderOnOpen && doorCollider != null)
      doorCollider.enabled = !open;
  }
}
