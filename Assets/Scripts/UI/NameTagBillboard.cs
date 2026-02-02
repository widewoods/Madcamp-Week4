using UnityEngine;

public class NameTagBillboard : MonoBehaviour
{
  [SerializeField] private Camera targetCamera;

  private void LateUpdate()
  {
    if (targetCamera == null)
      ResolveCamera();
    if (targetCamera == null) return;

    transform.forward = targetCamera.transform.forward;
  }

  private void ResolveCamera()
  {
    targetCamera = Camera.main;
    if (targetCamera != null) return;
    targetCamera = FindFirstObjectByType<Camera>(FindObjectsInactive.Exclude);
  }
}


