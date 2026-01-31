using Unity.Netcode;
using UnityEngine;

public class PlayerAnimDriver : NetworkBehaviour
{
  [SerializeField] Animator animator;
  [SerializeField] CharacterController cc;

  void Reset()
  {
    animator = GetComponentInChildren<Animator>();
    cc = GetComponent<CharacterController>();
  }

  void Update()
  {
    if (!IsOwner) return;
    if (animator == null || cc == null) return;

    Vector3 v = cc.velocity;
    v.y = 0f;

    animator.SetFloat("Speed", v.magnitude);
  }
}
