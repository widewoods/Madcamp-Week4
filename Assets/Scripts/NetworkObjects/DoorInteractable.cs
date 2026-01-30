using Unity.Netcode;
using UnityEngine;

public class DoorInteractable : InteractableBase
{
  public override void ServerInteract(ulong clientId)
  {
    Debug.Log($"Door interacted by {clientId}");
  }
}
