using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IMinigameUseHandler
{
  MinigameType MinigameType { get; }
  void OnUsePressed();
  void OnUseHeld();
  void OnUseReleased();
}

public class MinigameInputRouter : NetworkBehaviour
{
  [SerializeField] private MinigameType activeMinigame = MinigameType.None;

  private IMinigameUseHandler[] handlers;
  private bool wasPressed;

  public MinigameType ActiveMinigame => activeMinigame;

  private void Awake()
  {
    handlers = GetComponents<IMinigameUseHandler>();
  }

  private void Update()
  {
    if (!IsOwner) return;
    if (Keyboard.current == null) return;

    bool isPressed = Keyboard.current.fKey.isPressed;
    if (isPressed)
      CallHeld();
    if (isPressed && !wasPressed)
      CallPressed();
    if (!isPressed && wasPressed)
      CallReleased();
    wasPressed = isPressed;
  }

  public void SetActiveMinigame(MinigameType type)
  {
    if (!IsOwner) return;
    activeMinigame = type;
  }

  private void CallPressed()
  {
    foreach (var h in handlers)
      if (h != null && h.MinigameType == activeMinigame)
        h.OnUsePressed();
  }

  private void CallHeld()
  {
    foreach (var h in handlers)
      if (h != null && h.MinigameType == activeMinigame)
        h.OnUseHeld();
  }

  private void CallReleased()
  {
    foreach (var h in handlers)
      if (h != null && h.MinigameType == activeMinigame)
        h.OnUseReleased();
  }
}
