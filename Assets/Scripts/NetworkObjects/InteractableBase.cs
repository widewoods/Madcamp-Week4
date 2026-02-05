using Unity.Netcode;
using UnityEngine;

public abstract class InteractableBase : NetworkBehaviour
{
  [SerializeField] private string prompt = "상호작용";

  public virtual string Prompt => prompt;

  public abstract void ServerInteract(ulong clientId);
}
