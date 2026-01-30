using Unity.Netcode;
using UnityEngine;

public abstract class InteractableBase : NetworkBehaviour
{
  [SerializeField] private string prompt = "Interact";

  public virtual string Prompt => prompt;

  public abstract void ServerInteract(ulong clientId);
}
