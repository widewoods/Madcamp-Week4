using Unity.Netcode;
using UnityEngine;

public class NoteInteractable : InteractableBase
{
  [Header("Note")]
  [SerializeField] private int seatId = -1;
  [SerializeField] private SeatInteractable seat;

  public override string Prompt
  {
    get
    {
      if (seat != null && seat.IsOccupied) return "Occupied";
      return "Write Note";
    }
  }

  public override void ServerInteract(ulong clientId)
  {
    if (!IsServer) return;
    if (seat != null && seat.IsOccupied) return;
    OpenNoteClientRpc(clientId, seatId);
  }

  [Rpc(SendTo.ClientsAndHost)]
  private void OpenNoteClientRpc(ulong clientId, int seatId)
  {
    if (NetworkManager.Singleton == null) return;
    if (NetworkManager.Singleton.LocalClientId != clientId) return;

    var ui = FindFirstObjectByType<SeatNotesUI>(FindObjectsInactive.Include);
    if (ui == null) return;
    ui.Open(seatId);
  }
}
