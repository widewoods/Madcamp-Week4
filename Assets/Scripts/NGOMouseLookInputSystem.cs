using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NgoMouseLookInputSystem : NetworkBehaviour
{
  [Header("References")]
  [SerializeField] private Transform cameraPivot;         // pitch
  [SerializeField] private InputAction lookAction; // Vector2 (Mouse delta / Right stick)

  [Header("Sensitivity")]
  [SerializeField] private float sensX = 200f; // yaw
  [SerializeField] private float sensY = 200f; // pitch

  [Header("Clamp")]
  [SerializeField] private float minPitch = -80f;
  [SerializeField] private float maxPitch = 80f;

  [Header("Options")]
  [SerializeField] private bool lockCursorOnSpawn = true;
  [SerializeField] private bool invertY = false;

  private float yaw;
  private float pitch;

  public override void OnNetworkSpawn()
  {
    // Only the owning client should read input / control camera
    if (!IsOwner)
    {
      enabled = false;
      return;
    }

    if (lockCursorOnSpawn)
    {
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
    }

    // Init angles to avoid snapping
    yaw = transform.eulerAngles.y;

    if (cameraPivot != null)
    {
      pitch = NormalizeAngle(cameraPivot.localEulerAngles.x);
      pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    lookAction = InputSystem.actions.FindAction("Look");
  }


  private void Update()
  {
    if (!IsOwner) return;
    if (cameraPivot == null || lookAction == null) return;

    Vector2 look = lookAction.ReadValue<Vector2>();

    // Mouse delta is already "per-frame"-ish; scaling by deltaTime keeps feel stable across FPS.
    float mouseX = look.x * sensX * Time.deltaTime;
    float mouseY = look.y * sensY * Time.deltaTime;

    if (!invertY) mouseY = -mouseY;

    yaw += mouseX;
    pitch += mouseY;
    pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

    transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

    // Optional: ESC to unlock cursor (desktop convenience)
    if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
    {
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
    }
  }

  private static float NormalizeAngle(float angle)
  {
    while (angle > 180f) angle -= 360f;
    while (angle < -180f) angle += 360f;
    return angle;
  }
}
