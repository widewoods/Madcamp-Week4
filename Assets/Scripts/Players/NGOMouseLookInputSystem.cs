using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NgoMouseLookInputSystem : NetworkBehaviour
{
  [Header("References")]
  [SerializeField] private Transform cameraPivot;
  [SerializeField] private PlayerInput playerInput;
  [SerializeField] private InputAction lookAction;

  [Header("Sensitivity")]
  [SerializeField] private float sensX = 200f;
  [SerializeField] private float sensY = 200f;

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
    if (playerInput == null) playerInput = GetComponent<PlayerInput>();

    if (!IsOwner)
    {
      playerInput.enabled = false;
      enabled = false;
      return;
    }

    if (lockCursorOnSpawn)
    {
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
    }

    yaw = transform.eulerAngles.y;

    if (cameraPivot != null)
    {
      pitch = NormalizeAngle(cameraPivot.localEulerAngles.x);
      pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    playerInput.enabled = true;
    lookAction = playerInput.actions["Look"];
    lookAction.Enable();
  }

  private void OnDisable()
  {
    lookAction?.Disable();
  }


  private void Update()
  {
    if (!IsOwner) return;
    if (cameraPivot == null || lookAction == null) return;

    Vector2 look = lookAction.ReadValue<Vector2>();

    float mouseX = look.x * sensX * Time.deltaTime;
    float mouseY = look.y * sensY * Time.deltaTime;

    if (!invertY) mouseY = -mouseY;

    yaw += mouseX;
    pitch += mouseY;
    pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

    transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

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
