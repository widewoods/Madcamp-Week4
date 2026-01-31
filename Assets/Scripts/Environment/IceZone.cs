using UnityEngine;

public class IceZone : MonoBehaviour
{
  [SerializeField] private bool isIce = true;

  private void OnTriggerEnter(Collider other)
  {
    var mover = other.GetComponentInParent<NGOPlayerMovement>();
    if (mover == null) return;
    mover.SetIce(isIce);
  }

  private void OnTriggerExit(Collider other)
  {
    var mover = other.GetComponentInParent<NGOPlayerMovement>();
    if (mover == null) return;
    mover.SetIce(false);
  }
}
