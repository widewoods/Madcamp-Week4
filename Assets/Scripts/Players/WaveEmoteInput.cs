using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class WaveEmoteInput : NetworkBehaviour
{
  [SerializeField] private Animator animator;
  [SerializeField] private string waveTrigger = "Wave";

  private void Awake()
  {
    if (animator == null)
      animator = GetComponentInChildren<Animator>();
  }

  private void Update()
  {
    if (!IsOwner) return;
    if (!Keyboard.current.hKey.wasPressedThisFrame) return;

    RequestWaveServerRpc();
  }

  [Rpc(SendTo.Server)]
  private void RequestWaveServerRpc()
  {
    PlayWaveClientRpc();
  }

  [Rpc(SendTo.ClientsAndHost)]
  private void PlayWaveClientRpc()
  {
    if (animator == null)
      animator = GetComponentInChildren<Animator>();
    if (animator == null) return;
    if (string.IsNullOrWhiteSpace(waveTrigger)) return;

    animator.SetTrigger(waveTrigger);
  }
}
