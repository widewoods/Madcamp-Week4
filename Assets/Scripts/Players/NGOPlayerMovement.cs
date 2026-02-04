using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NGOPlayerMovement : NetworkBehaviour
{
  [SerializeField] float moveSpeed = 5f;
  [SerializeField] float acceleration = 12f;
  [SerializeField] float deceleration = 16f;
  [SerializeField] float iceAcceleration = 3f;
  [SerializeField] float iceDeceleration = 2f;
  [SerializeField] float rotateSpeed = 6f;

  [SerializeField] float gravity = -20f;
  [SerializeField] float jumpHeight = 1.5f;

  [SerializeField] PlayerInput playerInput;
  [SerializeField] Transform cameraPivot;
  [SerializeField] Transform model;
  [SerializeField] float modelYawOffset = 0f;

  CharacterController controller;
  InputAction moveAction;
  InputAction jumpAction;

  private float verticalVelocity;
  private Vector3 horizontalVelocity;
  private bool onIce;

  public override void OnNetworkSpawn()
  {
    if (playerInput == null) playerInput = GetComponent<PlayerInput>();

    if (!IsOwner)
    {
      playerInput.enabled = false;
      enabled = false;
      return;
    }

    playerInput.enabled = true;
    EnsureActions();
    EnableActions();
  }

  void OnEnable()
  {
    if (!IsOwner) return;
    if (playerInput == null) playerInput = GetComponent<PlayerInput>();
    EnsureActions();
    EnableActions();
  }

  void OnDisable()
  {
    moveAction?.Disable();
    jumpAction?.Disable();
  }

  void Awake()
  {
    controller = GetComponent<CharacterController>();
    if (controller == null)
      controller = gameObject.AddComponent<CharacterController>();
  }

  private void EnsureActions()
  {
    if (moveAction == null && playerInput != null)
      moveAction = playerInput.actions["Move"];
    if (jumpAction == null && playerInput != null)
      jumpAction = playerInput.actions["Jump"];
  }

  private void EnableActions()
  {
    moveAction?.Enable();
    jumpAction?.Enable();
  }

  void Update()
  {
    if (!IsOwner) return;
    if (!controller.enabled) return;

    Vector2 moveValue = moveAction.ReadValue<Vector2>();
    Vector3 input = new Vector3(moveValue.x, 0f, moveValue.y);
    if (input.sqrMagnitude > 1f) input.Normalize();


    Vector3 moveDir = cameraPivot.right * input.x + cameraPivot.forward * input.z;
    moveDir.y = 0f;

    if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

    if (moveDir.sqrMagnitude > 0.0001f && model != null)
    {
      Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up) * Quaternion.Euler(0f, modelYawOffset, 0f);
      model.rotation = Quaternion.Slerp(model.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }

    if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;

    if (controller.isGrounded && jumpAction.WasPressedThisFrame())
    {
      verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    verticalVelocity += gravity * 1.3f * Time.deltaTime;

    float accel = onIce ? iceAcceleration : acceleration;
    float decel = onIce ? iceDeceleration : deceleration;

    Vector3 targetHorizontal = moveDir * moveSpeed;
    float rate = targetHorizontal.sqrMagnitude > 0.001f ? accel : decel;
    horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetHorizontal, rate * Time.deltaTime);

    Vector3 velocity = horizontalVelocity;
    velocity.y = verticalVelocity;

    controller.Move(velocity * Time.deltaTime);
  }

  public void SetIce(bool value)
  {
    onIce = value;
  }

}
