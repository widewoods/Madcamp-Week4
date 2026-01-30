using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NGOPlayerMovement : NetworkBehaviour
{
  [SerializeField] float moveSpeed = 5f;

  [SerializeField] float gravity = -20f;
  [SerializeField] float jumpHeight = 1.5f;

  [SerializeField] PlayerInput playerInput;

  CharacterController controller;
  InputAction moveAction;
  InputAction jumpAction;

  private float verticalVelocity;

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


    Vector3 moveDir = transform.right * input.x + transform.forward * input.z;
    moveDir.y = 0f;
    if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

    if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;

    if (controller.isGrounded && jumpAction.WasPressedThisFrame())
    {
      verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    verticalVelocity += gravity * 1.3f * Time.deltaTime;

    Vector3 velocity = moveDir * moveSpeed;
    velocity.y = verticalVelocity;

    controller.Move(velocity * Time.deltaTime);
  }

}
