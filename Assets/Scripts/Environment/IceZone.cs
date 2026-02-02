using Unity.VisualScripting;
using UnityEngine;

public class IceZone : MonoBehaviour
{
  [SerializeField] private bool isIce = true;
  private Animator animator;

  private void OnTriggerEnter(Collider other)
  {
    var animator = other.GetComponentInChildren<Animator>();
    var mover = other.GetComponentInParent<NGOPlayerMovement>();
    if (mover == null) return;
    mover.SetIce(isIce);
    animator.SetBool("IsSkating", true);
  }

  private void OnTriggerExit(Collider other)
  {
    var animator = other.GetComponentInChildren<Animator>();
    var mover = other.GetComponentInParent<NGOPlayerMovement>();
    if (mover == null) return;
    mover.SetIce(false);
    animator.SetBool("IsSkating", false);

  }
}
